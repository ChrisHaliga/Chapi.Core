using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.DatabaseItemServices;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Services.ApiServices
{
    public class GroupApiService : ApiServiceBase<GroupWithId>
    {
        public GroupApiService(DatabaseUserService databaseUserService, DatabaseGroupService databaseGroupService, DatabaseApplicationService databaseApplicationService) : base(databaseUserService, databaseGroupService, databaseApplicationService) { }

        protected override IDatabaseItemService<GroupWithId> PrimaryDatabaseService { get => _databaseGroupService; }
        protected override GroupWithId CopyPrimaryDatabaseItemWithId(GroupWithId item) => new GroupWithId(item);
        protected override void SoftOverwriteWith(GroupWithId target, GroupWithId source) => target.SoftOverwriteWith(source);


        public override async Task<GroupWithId> MigrateItem(GroupWithId group, string newOrganization, CancellationToken cancellationToken = default)
        {
            group.Organization = newOrganization;

            await ValidateItem(group, cancellationToken);
            await _databaseGroupService.DeleteItem(group, cancellationToken);
            return await _databaseGroupService.CreateItem(group, cancellationToken);
        }

        public override async Task<RelatedData> ValidateItem(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var result = new RelatedData();

            await EnsureUsersExist(result, group, group.Users, cancellationToken);

            await EnsureOrganizationExists(result, group, group.Organization, cancellationToken);

            await EnsureParentExists(result, group, group.Parent, cancellationToken);

            await EnsureChildrenExist(result, group, group.Children, cancellationToken);

            await EnsureApplicationsAndRolesExist(result, group, group.Applications, cancellationToken);

            return result;
        }

        public override async Task UpdateRelatedItems(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken = default)
        {
            string groupId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException(before ?? after, "Group Id is required");
            string organization = before?.Organization ?? after?.Organization ?? throw new BadRequestException(before ?? after, "Organization required");

            await UpdateUsers(validatedData, before, after, cancellationToken);
            await UpdateOrganization(validatedData, before, after, cancellationToken);
            await UpdateParent(validatedData, before, after, cancellationToken);
            await UpdateChildren(validatedData, before, after, cancellationToken);
            await UpdateApplications(validatedData, before, after, cancellationToken);
        }

        private async Task UpdateUsers(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Users.Diff(after?.Users) : ([], after?.Users ?? [], []);

            foreach (var userId in removed ?? [])
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _databaseUserService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && user.Groups.AddIfNotExists(id))
                {
                    await _databaseUserService.PatchItem(user, cancellationToken);
                }
            }


            foreach (var userId in added)
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _databaseUserService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && user.Groups.Remove(id))
                {
                    await _databaseUserService.PatchItem(user, cancellationToken);
                }
            }
        }

        private async Task<GroupWithId?> UpdateOrganization(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken)
        {
            if (before == null && after == null) return before;

            string id = (before?.GetId() ?? after?.GetId())!;

            if (before?.Organization != after?.Organization)
            {
                if (!string.IsNullOrEmpty(before?.Organization) && !string.IsNullOrEmpty(after?.Organization))
                {
                    before = await MigrateItem(before, after.Organization, cancellationToken);
                }

                if (!string.IsNullOrEmpty(after?.Organization))
                {
                    await AddGroupToChildren(validatedData, Group.IdAsOrganization(after.Organization), id, cancellationToken);
                }

                if (!string.IsNullOrEmpty(before?.Organization))
                {
                    await RemoveGroupFromChildren(validatedData, Group.IdAsOrganization(before.Organization), id, cancellationToken);
                }
            }

            return before;
        }

        private async Task UpdateParent(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;

            if (before?.Parent != after?.Parent)
            {
                if (after?.Parent != null)
                {
                    await AddGroupToChildren(validatedData, after?.Parent, id, cancellationToken);
                }

                if (before?.Parent != null)
                {
                    await RemoveGroupFromChildren(validatedData, before?.Parent, id, cancellationToken);
                }
            }
        }

        private async Task UpdateChildren(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Children.Diff(after?.Children) : ([], after?.Children ?? [], []);

            foreach (var childGroupId in added)
            {
                if (!string.IsNullOrEmpty(childGroupId))
                {
                    var childGroup = validatedData?.Groups?.TryGetNullable(childGroupId) ?? await _databaseGroupService.GetItem(new GroupWithId(childGroupId), cancellationToken);

                    if (childGroup != null && childGroup.Parent != id)
                    {
                        childGroup.Parent = id;
                        await _databaseGroupService.PatchItem(childGroup, cancellationToken);
                    }
                }
            }

            foreach (var childGroupId in removed)
            {
                if (!string.IsNullOrEmpty(childGroupId))
                {
                    var childGroup = validatedData?.Groups?.TryGetNullable(childGroupId) ?? await _databaseGroupService.GetItem(new GroupWithId(childGroupId), cancellationToken);
                    if (childGroup != null && childGroup.Parent == id)
                    {
                        childGroup.Parent = before?.Parent ?? after?.Parent;
                        await _databaseGroupService.PatchItem(childGroup, cancellationToken);
                    }
                }
            }
        }


        private async Task UpdateApplications(RelatedData? validatedData, GroupWithId? before, GroupWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Applications.Diff(after?.Applications, (x1, x2) => x1.Name == x2.Name) : ([], after?.Applications ?? [], []);

            foreach (var applicationAccess in added)
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications?.TryGetNullable(applicationAccess.Name) ?? await _databaseApplicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                    if (application != null && application.Groups.AddIfNotExists(id))
                    {
                        await _databaseApplicationService.PatchItem(application, cancellationToken);
                    }
                }
            }

            foreach (var applicationAccess in removed)
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications?.TryGetNullable(applicationAccess.Name) ?? await _databaseApplicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);
                    if (application != null && application.Groups.Remove(id))
                    {
                        await _databaseApplicationService.PatchItem(application, cancellationToken);
                    }
                }
            }
        }


        private async Task AddGroupToChildren(RelatedData? validatedData, string groupId, string itemId, CancellationToken cancellationToken)
        {
            var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

            if (group != null && group.Children.AddIfNotExists(itemId))
            {
                await _databaseGroupService.PatchItem(group, cancellationToken);
            }
        }

        private async Task RemoveGroupFromChildren(RelatedData? validatedData, string groupId, string itemId, CancellationToken cancellationToken)
        {
            var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

            if (group != null && group.Children.Remove(itemId))
            {
                await _databaseGroupService.PatchItem(group, cancellationToken);
            }
        }
    }
}
