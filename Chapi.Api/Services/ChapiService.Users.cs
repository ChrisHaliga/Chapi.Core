using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;
using Microsoft.Azure.Cosmos;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Chapi.Api.Services
{
    public sealed partial class ChapiService : IChapiServiceUser
    {
        public async Task<UserWithId> GetUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            return await _userService.GetItem(user, cancellationToken);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken = default)
        {
            return await _userService.GetItemsWhereKeyIsValue(nameof(UserWithId.Organization), organization, cancellationToken);
        }

        public async Task<List<UserWithId>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            return await _userService.GetAllItems(cancellationToken);
        }

        public async Task<UserWithId> CreateUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateUser(user, cancellationToken);

            await UpdateUsersRelatedItems(null, user, validatedData, cancellationToken);

            return await _userService.CreateItem(user, cancellationToken);
        }

        public async Task<UserWithId> PatchUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userService.GetItem(user, cancellationToken);
            var newUser = new UserWithId(existingUser);
            newUser.SoftOverwrite(user);

            var validatedData = await ValidateUser(newUser, cancellationToken);

            await UpdateUsersRelatedItems(existingUser, newUser, validatedData, cancellationToken);

            return await _userService.UpdateItem(newUser, hard:false, cancellationToken);
        }

        public async Task<UserWithId> PutUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userService.GetItem(user);
            var validatedData = await ValidateUser(user, cancellationToken);

            await UpdateUsersRelatedItems(existingUser, user, validatedData, cancellationToken);

            return await _userService.UpdateItem(user, hard:true, cancellationToken);
        }

        public async Task DeleteUser(UserWithId userQuery, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetItem(userQuery, cancellationToken);

            await UpdateUsersRelatedItems(user, null, null, cancellationToken);
            await _userService.DeleteItem(user, cancellationToken);
        }

        public async Task<UserWithId> MigrateUser(UserWithId user, string newOrganization, CancellationToken cancellationToken = default)
        {
            var testUser = new UserWithId(user);

            testUser.Email = "thisShouldNeverBeReal" + user.Email;
            testUser.Id = testUser.Email;
            testUser.Organization = newOrganization;

            await ValidateUser(testUser);

            user.Organization = newOrganization;

            await _userService.DeleteItem(user, cancellationToken);
            return await _userService.CreateItem(user, cancellationToken);

        }

        public async Task<ChapiData> ValidateUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            if(string.IsNullOrEmpty(user.Email))
            {
                throw new BadRequestException(user, "User must have an email");
            }
            if (string.IsNullOrEmpty(user.Organization))
            {
                throw new BadRequestException(user, "User must have an organization");
            }

            await EnsureOrganizationExists(user, user.Organization, result, cancellationToken);
            await EnsureGroupsExist(user, user.Groups, result, cancellationToken);
            await EnsureApplicationsAndRolesExist(user, user.Applications, result, cancellationToken);

            return result;
        }

        private async Task UpdateUsersRelatedItems(UserWithId? before, UserWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            string userId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException("Updating null to null???");
            string organization = before?.Organization ?? after?.Organization ?? throw new BadRequestException(before ?? after, "Organization required");

            if (before?.Organization != after?.Organization)
            {
                if (!string.IsNullOrEmpty(before?.Organization) && !string.IsNullOrEmpty(after?.Organization))
                {
                    before = await MigrateUser(before, after.Organization, cancellationToken);
                }

                if (after?.Organization != null)
                {
                    await AddItemToOrganization(validatedData, after?.Organization, userId, AddMemberToGroup, cancellationToken);
                }

                if (before?.Organization != null)
                {
                    await RemoveItemFromOrganization(validatedData, before?.Organization, userId, RemoveMemberFromGroup, cancellationToken);
                }
            }

            await UpdateGroups_User(validatedData, before, after, cancellationToken); //TODO - all of these should be like this

            await AddItemToApplications(validatedData, after?.Applications, userId, AddUserToApplication, cancellationToken);
            await RemoveItemFromApplicationsBeforeChange(validatedData, before?.Applications, after?.Applications, userId, RemoveUserFromApplication, cancellationToken);
        }
        private async Task UpdateGroups_User(ChapiData? validatedData, UserWithId? before, UserWithId? after, CancellationToken cancellationToken = default)
        {
            if(before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Groups.Diff(after?.Groups) : ([], after?.Groups ?? [], []);

            foreach (var groupId in added)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && AddMemberToGroup(group, id))
                {
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }

            foreach (var groupId in removed)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && (await RemoveMemberFromGroupAsync(group, id, cancellationToken)))
                {
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }
        }

        private bool AddMemberToGroup(GroupWithId group, string id)
        {
            if (!group.Members.Contains(id))
            {
                group.Members.Add(id);
                return true;
            }
            return false;
        }

        private bool RemoveMemberFromGroup(GroupWithId group, string id)
        {
            if (group.Members.Contains(id))
            {
                group.Members.Remove(id);
                return true;
            }
            return false;
        }

        private async Task<bool> RemoveMemberFromGroupAsync(GroupWithId group, string id, CancellationToken cancellationToken)
        {
            if(group.IsOrganization())
            {
                await DeleteUser(new UserWithId(id), cancellationToken);
                return false;
            }

            return RemoveMemberFromGroup(group, id);
        }

        private bool AddUserToApplication(ApplicationWithId application, string id)
        {
            if (!application.Users.Contains(id))
            {
                application.Users.Add(id);
                return true;
            }
            return false;
        }

        private bool RemoveUserFromApplication(ApplicationWithId application, string id)
        {
            if (application.Users.Contains(id))
            {
                application.Users.Remove(id);
                return true;
            }
            return false;
        }
    }
}
