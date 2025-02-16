using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.DatabaseItemServices;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Services.ApiServices
{
    public class UserApiService : ApiServiceBase<UserWithId>
    {
        public UserApiService(DatabaseUserService databaseUserService, DatabaseGroupService databaseGroupService, DatabaseApplicationService databaseApplicationService): base(databaseUserService,databaseGroupService,databaseApplicationService){}

        protected override IDatabaseItemService<UserWithId> PrimaryDatabaseService { get => _databaseUserService; }
        protected override UserWithId CopyPrimaryDatabaseItemWithId(UserWithId item) => new UserWithId(item);
        protected override void SoftOverwriteWith(UserWithId target, UserWithId source) => target.SoftOverwriteWith(source);

        public override async Task<UserWithId> MigrateItem(UserWithId user, string newOrganization, CancellationToken cancellationToken = default)
        {
            var testUser = new UserWithId(user);

            testUser.Email = "thisShouldNeverBeReal" + user.Email;
            testUser.Id = testUser.Email;
            testUser.Organization = newOrganization;

            await ValidateItem(testUser);

            user.Organization = newOrganization;

            await _databaseUserService.DeleteItem(user, cancellationToken);
            return await _databaseUserService.CreateItem(user, cancellationToken);
        }

        public override async Task<RelatedData> ValidateItem(UserWithId user, CancellationToken cancellationToken = default)
        {
            var result = new RelatedData();

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new BadRequestException(user, "User must have an email");
            }
            if (string.IsNullOrEmpty(user.Organization))
            {
                throw new BadRequestException(user, "User must have an organization");
            }

            await EnsureOrganizationExists(result, user, user.Organization, cancellationToken);
            await EnsureChildrenExist(result, user, user.Groups, cancellationToken);
            await EnsureApplicationsAndRolesExist(result, user, user.Applications, cancellationToken);

            return result;
        }

        public override async Task UpdateRelatedItems(RelatedData? validatedData, UserWithId? before, UserWithId? after, CancellationToken cancellationToken = default)
        {
            string userId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException("Updating null to null???");
            string organization = before?.Organization ?? after?.Organization ?? throw new BadRequestException(before ?? after, "Organization required");
           
            before = await UpdateOrganization(validatedData, before, after, userId, cancellationToken);

            await UpdateGroups(validatedData, before, after, cancellationToken);

            await UpdateApplications(validatedData, before, after, cancellationToken);
        }

        private async Task<UserWithId?> UpdateOrganization(RelatedData? validatedData, UserWithId? before, UserWithId? after, string userId, CancellationToken cancellationToken)
        {
            if (before?.Organization != after?.Organization)
            {
                if (!string.IsNullOrEmpty(before?.Organization) && !string.IsNullOrEmpty(after?.Organization))
                {
                    before = await MigrateItem(before, after.Organization, cancellationToken);
                }

                if (after?.Organization != null)
                {
                    await AddUserToOrganization(validatedData, after?.Organization, userId, cancellationToken);
                }

                if (before?.Organization != null)
                {
                    await RemoveUserFromOrganization(validatedData, before?.Organization, userId, cancellationToken);
                }
            }

            return before;
        }

        private async Task UpdateGroups(RelatedData? validatedData, UserWithId? before, UserWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Groups.Diff(after?.Groups) : ([], after?.Groups ?? [], []);

            foreach (var groupId in added)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && group.Users.AddIfNotExists(id))
                {
                    await _databaseGroupService.PatchItem(group, cancellationToken);
                }
            }

            foreach (var groupId in removed)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && group.Users.Remove(id))
                {
                    await _databaseGroupService.PatchItem(group, cancellationToken);
                }
            }
        }

        private async Task UpdateApplications(RelatedData? validatedData, UserWithId? before, UserWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Applications.Diff(after?.Applications, (x1, x2) => x1.Name == x2.Name) : ([], after?.Applications ?? [], []);

            foreach (var applicationAccess in added)
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications?.TryGetNullable(applicationAccess.Name) ?? await _databaseApplicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                    if (application != null && application.Users.AddIfNotExists(id))
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
                    if (application != null && application.Users.Remove(id))
                    {
                        await _databaseApplicationService.PatchItem(application, cancellationToken);
                    }
                }
            }
        }

        private async Task AddUserToOrganization(RelatedData? validatedData, string? organizationName, string itemId, CancellationToken cancellationToken)
        {
            var organizationId = Group.IdAsOrganization(organizationName);
            var organization = validatedData?.Groups?.TryGetNullable(organizationId) ?? await _databaseGroupService.GetItem(new GroupWithId(organizationId), cancellationToken);

            if (organization != null && organization.Users.AddIfNotExists(itemId))
            {
                await _databaseGroupService.PatchItem(organization, cancellationToken);
            }
        }

        private async Task RemoveUserFromOrganization(RelatedData? validatedData, string? organizationName, string itemId, CancellationToken cancellationToken)
        {
            var organizationId = Group.IdAsOrganization(organizationName);
            var organization = validatedData?.Groups?.TryGetNullable(organizationId) ?? await _databaseGroupService.GetItem(new GroupWithId(organizationId), cancellationToken);

            if (organization != null && organization.Users.Remove(itemId))
            {
                await _databaseGroupService.PatchItem(organization, cancellationToken);
            }
        }
    }
}
