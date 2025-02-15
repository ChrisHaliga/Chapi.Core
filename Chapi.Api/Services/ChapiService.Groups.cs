using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;
using Microsoft.Azure.Cosmos;
using System.Text.RegularExpressions;

namespace Chapi.Api.Services
{
    public sealed partial class ChapiService : IChapiServiceGroup
    {
        public async Task<GroupWithId> GetGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            return await _groupService.GetItem(group, cancellationToken);
        }

        public async Task<List<GroupWithId>> GetGroupsByOrganization(string organization, CancellationToken cancellationToken = default)
        {
            return await _groupService.GetItemsWhereKeyIsValue(nameof(GroupWithId.Organization), organization, cancellationToken);
        }

        public async Task<List<GroupWithId>> GetAllGroups(CancellationToken cancellationToken = default)
        {
            return await _groupService.GetAllItems(cancellationToken);
        }

        public async Task<GroupWithId> CreateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateGroup(group, cancellationToken);

            await UpdateGroupsRelatedItems(null, group, validatedData, cancellationToken);

            return await _groupService.CreateItem(group, cancellationToken);
        }

        public async Task<GroupWithId> PatchGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var existingGroup = await _groupService.GetItem(group, cancellationToken);
            var newGroup = new GroupWithId(existingGroup);
            newGroup.SoftOverwrite(group);

            var validatedData = await ValidateGroup(newGroup, cancellationToken);

            await UpdateGroupsRelatedItems(existingGroup, newGroup, validatedData, cancellationToken);

            return await _groupService.UpdateItem(newGroup, hard: false, cancellationToken);
        }

        public async Task<GroupWithId> PutGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var existingGroup = await _groupService.GetItem(group);
            var validatedData = await ValidateGroup(group, cancellationToken);

            await UpdateGroupsRelatedItems(existingGroup, group, validatedData, cancellationToken);

            return await _groupService.UpdateItem(group, hard: true, cancellationToken);
        }

        public async Task DeleteGroup(GroupWithId groupQuery, CancellationToken cancellationToken = default)
        {
            var group = await _groupService.GetItem(groupQuery, cancellationToken);
            await UpdateGroupsRelatedItems(group, null, null, cancellationToken);
            await _groupService.DeleteItem(group, cancellationToken);
        }

        public async Task<ChapiData> ValidateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            await EnsureUsersExist(group, group.Members, result, cancellationToken);

            await EnsureOrganizationExists(group, group.Organization, result, cancellationToken);

            await EnsureParentExists(group, group.Parent, result, cancellationToken);

            await EnsureGroupsExist(group, group.Children, result, cancellationToken);

            await EnsureApplicationsAndRolesExist(group, group.Applications, result, cancellationToken);

            return result;
        }

        private async Task UpdateGroupsRelatedItems(GroupWithId? before, GroupWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            string groupId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException(before ?? after, "Group Id is required");
            string organization = before?.Organization ?? after?.Organization ?? throw new BadRequestException(before ?? after, "Organization required");

            await AddItemToUsers(validatedData, after?.Members, groupId, AddGroupToUser, cancellationToken);
            await RemoveItemFromUsersBeforeChangeAsync(validatedData, before?.Members, after?.Members, groupId, RemoveGroupFromUserAsync, cancellationToken);

            if (before?.Organization != null && after?.Organization != null && before.Organization != after.Organization)
            {
                before = await MigrateGroup(before, after.Organization, cancellationToken);

                if (after?.Organization != null)
                {
                    await AddItemToOrganization(validatedData, after?.Organization, groupId, AddGroupToParent, cancellationToken);
                }

                if (before?.Organization != null)
                {
                    await RemoveItemFromOrganization(validatedData, after?.Organization, groupId, RemoveGroupFromParent, cancellationToken);
                }
            }

            if (!string.IsNullOrEmpty(after?.Parent))
            {
                await AddItemToGroup(validatedData, after.Parent, groupId, AddGroupToParent, cancellationToken);

                if(before?.Parent != null && before.Parent != after.Parent)
                {
                    await RemoveItemFromGroup(validatedData, before.Parent, groupId, RemoveGroupFromParent, cancellationToken);
                }
            }
            await UpdateGroups_Groups(validatedData, before, after, cancellationToken);

            await AddItemToApplications(validatedData, after?.Applications, groupId, AddGroupToApplication, cancellationToken);
            await RemoveItemFromApplicationsBeforeChange(validatedData, before?.Applications, after?.Applications, groupId, RemoveGroupFromApplication, cancellationToken);
        }

        public async Task<GroupWithId> MigrateGroup(GroupWithId item, string newPartition, CancellationToken cancellationToken = default)
        {
            item.Organization = newPartition;

            await ValidateGroup(item, cancellationToken);
            await _groupService.DeleteItem(item, cancellationToken);
            return await _groupService.CreateItem(item, cancellationToken);
        }

        private bool AddGroupToUser(UserWithId user, string id)
        {
            if (!user.Groups.Contains(id))
            {
                user.Groups.Add(id);
                return true;
            }
            return false;
        }

        private async Task UpdateGroups_Groups(ChapiData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Children.Diff(after?.Children) : ([], after?.Children ?? [], []);

            foreach (var groupId in added)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (!string.IsNullOrEmpty(group?.Parent) && group.Parent != id)
                {
                    var groupOldParent = validatedData?.Groups?.TryGetNullable(group.Parent) ?? await _groupService.GetItem(new GroupWithId(group.Parent), cancellationToken);

                    RemoveGroupFromParent(groupOldParent, groupId);
                    AddGroupToParent(groupOldParent, id);

                    await _groupService.PatchItem(groupOldParent, cancellationToken);
                }

                if (group != null && AddGroupToChild(group, id))
                {
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }

            foreach (var groupId in removed)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);
                var parentId = (before?.Parent ?? after?.Parent);

                if (!string.IsNullOrEmpty(parentId))
                {
                    var parent = validatedData?.Groups?.TryGetNullable(parentId) ?? await _groupService.GetItem(new GroupWithId(parentId), cancellationToken);

                    RemoveGroupFromParent(parent, id);
                    AddGroupToParent(parent, groupId);

                    await _groupService.PatchItem(parent, cancellationToken);
                }

                if (group != null && (await RemoveGroupFromChildAsync(group, (before ?? after)!, cancellationToken)))
                {
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }
        }

        private async Task<bool> RemoveGroupFromUserAsync(UserWithId user, string groupId, CancellationToken cancellationToken)
        {
            if (user.Organization == groupId)
            {
                await DeleteUser(user, cancellationToken);
                return false;
            }

            if (user.Groups.Contains(groupId))
            {
                user.Groups.Remove(groupId);
                return true;
            }
            return false;
        }

        private bool AddGroupToParent(GroupWithId parentGroup, string childId)
        {
            if (!parentGroup.Children.Contains(childId))
            {
                parentGroup.Children.Add(childId);
                return true;
            }
            return false;
        }

        private bool RemoveGroupFromParent(GroupWithId parentGroup, string childId)
        {
            if (parentGroup.Children.Contains(childId))
            {
                parentGroup.Children.Remove(childId);
                return true;
            }
            return false;
        }

        private bool AddGroupToChild(GroupWithId childGroup, string parentId)
        {
            if (childGroup.Parent != parentId)
            {
                childGroup.Parent = parentId;
                return true;
            }
            return false;
        }

        private async Task<bool> RemoveGroupFromChildAsync(GroupWithId childGroup, GroupWithId parentGroup, CancellationToken cancellationToken)
        {
            if (childGroup.Organization == parentGroup.Name)
            {
                await DeleteGroup(childGroup, cancellationToken);
                return false;
            }

            if (childGroup.Parent == parentGroup.GetId())
            {
                childGroup.Parent = parentGroup.Parent;
                return true;
            }
            return false;
        }

 
        private bool AddGroupToApplication(ApplicationWithId application, string groupId)
        {
            if (!application.GroupsIds.Contains(groupId))
            {
                application.GroupsIds.Add(groupId);
                return true;
            }
            return false;
        }


        private bool RemoveGroupFromApplication(ApplicationWithId application, string groupId)
        {
            if (application.GroupsIds.Contains(groupId))
            {
                application.GroupsIds.Remove(groupId);
                return true;
            }
            return false;
        }
    }
}
