using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.CrudServices;
using Chapi.Api.Utilities.Extensions;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using System.Text.RegularExpressions;
using static Chapi.Api.Services.ChapiService;

namespace Chapi.Api.Services
{
    interface IChapiServiceUser
    {
        Task<UserWithId> GetUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<List<UserWithId>> GetUsersByOrganization(string parent, CancellationToken cancellationToken = default);
        Task<List<UserWithId>> GetAllUsers(CancellationToken cancellationToken = default);
        Task<ChapiData> ValidateUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> CreateUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> PatchUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> PutUser(UserWithId user, CancellationToken cancellationToken = default);
        Task DeleteUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> MigrateUser(UserWithId item, string newPartition, CancellationToken cancellationToken = default);
    }
    interface IChapiServiceGroup
    {
        Task<GroupWithId> GetGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<List<GroupWithId>> GetGroupsByOrganization(string organization, CancellationToken cancellationToken = default);
        Task<List<GroupWithId>> GetAllGroups(CancellationToken cancellationToken = default);
        Task<ChapiData> ValidateGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> CreateGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> PatchGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> PutGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task DeleteGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> MigrateGroup(GroupWithId user, string newPartition, CancellationToken cancellationToken = default);
    }
    interface IChapiServiceApplication
    {
        Task<ApplicationWithId> GetApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<List<ApplicationWithId>> GetApplicationsByPlatform(string platform, CancellationToken cancellationToken = default);
        Task<List<ApplicationWithId>> GetAllApplications(CancellationToken cancellationToken = default);
        Task<ChapiData> ValidateApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> CreateApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> PatchApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> PutApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task DeleteApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> MigrateApplication(ApplicationWithId application, string newPartition, CancellationToken cancellationToken = default);
    }

    public sealed partial class ChapiService
    {
        private readonly UserService _userService;
        private readonly GroupService _groupService;
        private readonly ApplicationService _applicationService;

        public ChapiService(UserService userService, GroupService groupService, ApplicationService applicationService)
        {
            _userService = userService;
            _groupService = groupService;
            _applicationService = applicationService;
        }

        public class ChapiData //This is mostly just to save db calls when validating before taking action
        {
            public Dictionary<string, UserWithId> Users { get; set; } = new Dictionary<string, UserWithId>();
            public Dictionary<string, GroupWithId> Groups { get; } = new Dictionary<string, GroupWithId>();
            public Dictionary<string, ApplicationWithId> Applications { get; } = new Dictionary<string, ApplicationWithId>();
        }

        private async Task EnsureUsersExist(IDatabaseItem item, List<string> userIds, ChapiData result, CancellationToken cancellationToken)
        {
            foreach (var userId in userIds)
            {
                var existingUser = (await _userService.GetItemIfExists(new UserWithId(userId), cancellationToken)) ?? throw new NotFoundException(item, $"User \"{userId}\" does not exist");

                result.Users[existingUser.GetId()] = existingUser;
            }
        }

        private async Task EnsureOrganizationExists(IDatabaseItem item, string? organizationId, ChapiData result, CancellationToken cancellationToken)
        {
            var itemAsGroup = item as GroupWithId;
            if (itemAsGroup?.IsOrganization() ?? false)
            {
                return;
            }

            var existingOrganization = (await _groupService.GetItemIfExists(new GroupWithId(organizationId, organizationId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{organizationId}\" does not exist");
            if (existingOrganization.Name != existingOrganization.Organization)
            {
                throw new NotFoundException(item, $"Group \"{organizationId}\" is not a organization");
            }
        }

        private async Task EnsureParentExists(IDatabaseItem item, string? parentId, ChapiData result, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return; //Having no parent is valid
            }

            var existingParent = (await _groupService.GetItem(new GroupWithId(parentId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{parentId}\" does not exist");
            if (existingParent == null)
            {
                throw new NotFoundException(item, $"Group \"{existingParent}\" is not an organization");
            }

            result.Groups[existingParent.GetId()] = existingParent;
        }

        private async Task EnsureGroupsExist(IDatabaseItem item, List<string> groupIds, ChapiData result, CancellationToken cancellationToken)
        {
            foreach (var groupId in groupIds)
            {
                var existingGroup = (await _groupService.GetItemIfExists(new GroupWithId(groupId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{groupId}\" does not exist");

                result.Groups[existingGroup.GetId()] = existingGroup;
            }
        }

        private async Task EnsureApplicationsAndRolesExist(IDatabaseItem item, List<ApplicationAccess> applicationAccesses, ChapiData result, CancellationToken cancellationToken)
        {
            foreach (var applicationAccess in applicationAccesses)
            {
                var existingApplication = await _applicationService.GetItemIfExists(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                if (existingApplication == null)
                {
                    throw new NotFoundException(item, $"Application \"{applicationAccess.Name}\" does not exist");
                }

                foreach (var role in applicationAccess.Roles)
                {
                    if (existingApplication.Roles.Find(x => x.Name == role) == null)
                    {
                        throw new NotFoundException(item, $"Role \"{role}\" for Application \"{applicationAccess.Name}\" does not exist");
                    }
                }
                result.Applications[existingApplication.GetId()] = existingApplication;
            }
        }

        private async Task AddItemToUsers(ChapiData? validatedData, List<string>? userIds, string itemId, Func<UserWithId, string, bool> addItemIfNotExists, CancellationToken cancellationToken)
        {
            foreach (var userId in userIds ?? [])
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _userService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && addItemIfNotExists(user, itemId))
                {
                    await _userService.PatchItem(user, cancellationToken);
                }
            }
        }

        private async Task AddItemToGroups(ChapiData? validatedData, List<string>? groupIds, string itemId, Func<GroupWithId, string, bool> addItemIfNotExists, CancellationToken cancellationToken)
        {
            foreach (var groupId in groupIds ?? [])
            {
                await AddItemToGroup(validatedData, groupId, itemId, addItemIfNotExists, cancellationToken);
            }
        }

        private async Task AddItemToGroup(ChapiData? validatedData, string groupId, string itemId, Func<GroupWithId, string, bool> addItemIfNotExists, CancellationToken cancellationToken)
        {
            var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

            if (group != null && addItemIfNotExists(group, itemId))
            {
                await _groupService.PatchItem(group, cancellationToken);
            }
        }

        private async Task AddItemToOrganization(ChapiData? validatedData, string? organizationId, string itemId, Func<GroupWithId, string, bool> addItemIfNotExists, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(organizationId) && itemId == Models.Group.IdAsOrganization(organizationId))
            {
                return; //This is an organization
            }

            await AddItemToGroups(validatedData, [new GroupWithId(organizationId, organizationId).GetId()], itemId, addItemIfNotExists, cancellationToken);
        }

        private async Task AddItemToApplications(
            ChapiData? validatedData,
            List<ApplicationAccess>? applicationAccesses,
            string itemId,
            Func<ApplicationWithId, string, bool> addItemIfNotExists,
            CancellationToken cancellationToken
        )
        {
            foreach (var applicationAccess in applicationAccesses ?? [])
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications?.TryGetNullable(applicationAccess.Name) ?? await _applicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                    if (application != null && addItemIfNotExists(application, itemId))
                    {
                        await _applicationService.PatchItem(application, cancellationToken);
                    }
                }
            }
        }

        private async Task RemoveItemFromUsersBeforeChange(ChapiData? validatedData, List<string>? beforeUsers, List<string>? afterUsers, string itemId, Func<UserWithId, string, bool> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            List<string> usersToRemove = (beforeUsers ?? []).Where(x => !(afterUsers ?? []).Contains(x)).ToList();

            foreach (var userId in usersToRemove)
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _userService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && removeItemIfExistsAsync(user, itemId))
                {
                    await _userService.PatchItem(user, cancellationToken);
                }
            }
        }

        private async Task RemoveItemFromUsersBeforeChangeAsync(ChapiData? validatedData, List<string>? beforeUsers, List<string>? afterUsers, string itemId, Func<UserWithId, string, CancellationToken, Task<bool>> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            List<string> usersToRemove = (beforeUsers ?? []).Where(x => !(afterUsers ?? []).Contains(x)).ToList();

            foreach (var userId in usersToRemove)
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _userService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && (await removeItemIfExistsAsync(user, itemId, cancellationToken)))
                {
                    await _userService.PatchItem(user, cancellationToken);
                }
            }
        }

        private async Task RemoveItemFromGroup(ChapiData? validatedData, string groupId, string itemId, Func<GroupWithId, string, bool> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

            if (group != null && removeItemIfExistsAsync(group, itemId))
            {
                await _groupService.PatchItem(group, cancellationToken);
            }
        }

        private async Task RemoveItemFromOrganization(ChapiData? validatedData, string? organizationId, string itemId, Func<GroupWithId, string, bool> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(organizationId) && itemId == Models.Group.IdAsOrganization(organizationId))
            {
                return; //This is an organization
            }

            await RemoveItemFromGroup(validatedData, new GroupWithId(organizationId, organizationId).GetId(), itemId, removeItemIfExistsAsync, cancellationToken);
        }

        private async Task RemoveItemFromGroupsBeforeChange(ChapiData? validatedData, List<string>? beforeGroups, List<string>? afterGroups, string itemId, Func<GroupWithId, string, bool> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            List<string> groupsToRemove = (beforeGroups ?? []).Where(x => !(afterGroups ?? []).Contains(x)).ToList();

            foreach (var groupId in groupsToRemove)
            {
                await RemoveItemFromGroup(validatedData, groupId, itemId, removeItemIfExistsAsync, cancellationToken);
            }
        }

        private async Task RemoveItemFromGroupAsync(ChapiData? validatedData, string groupId, GroupWithId item, Func<GroupWithId, GroupWithId, CancellationToken, Task<bool>> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

            if (group != null && (await removeItemIfExistsAsync(group, item, cancellationToken)))
            {
                await _groupService.PatchItem(group, cancellationToken);
            }
        }

        private async Task RemoveItemsFromGroup(ChapiData? validatedData, List<string>? groupsToRemove, GroupWithId item, Func<GroupWithId, GroupWithId, CancellationToken, Task<bool>> removeItemIfExistsAsync, CancellationToken cancellationToken)
        {
            foreach (var groupId in groupsToRemove ?? [])
            {
                await RemoveItemFromGroupAsync(validatedData, groupId, item, removeItemIfExistsAsync, cancellationToken);
            }
        }

        private async Task RemoveItemFromApplicationsBeforeChange(
            ChapiData? validatedData,
            List<ApplicationAccess>? beforeApplications,
            List<ApplicationAccess>? afterApplications,
            string itemId,
            Func<ApplicationWithId, string, bool> addItemIfNotExists,
            CancellationToken cancellationToken
        )
        {
            List<ApplicationAccess> applicationsToRemove = (beforeApplications ?? []).Where(x => (afterApplications ?? []).Find(y => x.Name == y.Name) == null).ToList();
            foreach (var applicationAccess in applicationsToRemove)
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications?.TryGetNullable(applicationAccess.Name) ?? await _applicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);
                    if (application != null && addItemIfNotExists(application, itemId))
                    {
                        await _applicationService.PatchItem(application, cancellationToken);
                    }
                }
            }
        }
    }
}
