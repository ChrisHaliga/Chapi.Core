using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.DatabaseItemServices;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Services.ApiServices
{
    public class ApplicationApiService : ApiServiceBase<ApplicationWithId>
    {
        public ApplicationApiService(DatabaseUserService databaseUserService, DatabaseGroupService databaseGroupService, DatabaseApplicationService databaseApplicationService) : base(databaseUserService, databaseGroupService, databaseApplicationService) { }

        protected override IDatabaseItemService<ApplicationWithId> PrimaryDatabaseService { get => _databaseApplicationService; }
        protected override ApplicationWithId CopyPrimaryDatabaseItemWithId(ApplicationWithId item) => new ApplicationWithId(item);
        protected override void SoftOverwriteWith(ApplicationWithId target, ApplicationWithId source) => target.SoftOverwriteWith(source);


        public override async Task<ApplicationWithId> MigrateItem(ApplicationWithId application, string newPlatform, CancellationToken cancellationToken = default)
        {
            application.Platform = newPlatform;

            await ValidateItem(application, cancellationToken);
            await _databaseApplicationService.DeleteItem(application, cancellationToken);
            return await _databaseApplicationService.CreateItem(application, cancellationToken);
        }

        public override async Task<RelatedData> ValidateItem(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var result = new RelatedData();

            await EnsureUsersExist(result, application, application.Users, cancellationToken);

            await EnsureChildrenExist(result, application, application.Groups, cancellationToken);

            return result;
        }

        public override async Task UpdateRelatedItems(RelatedData? validatedData, ApplicationWithId? before, ApplicationWithId? after, CancellationToken cancellationToken = default)
        {
            string applicationId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException("Updating null to null???");

            await UpdateUsers(validatedData, before, after, cancellationToken);
            await UpdateGroups(validatedData, before, after, cancellationToken);
        }

        private async Task UpdateUsers(RelatedData? validatedData, ApplicationWithId? before, ApplicationWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Users.Diff(after?.Users) : ([], after?.Users ?? [], []);

            var applicationAccess = new ApplicationAccess() { Name = id };

            foreach (var userId in removed ?? [])
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _databaseUserService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && user.Applications.AddIfNotExists(applicationAccess, x => x.Name == applicationAccess.Name))
                {
                    await _databaseUserService.PatchItem(user, cancellationToken);
                }
            }


            foreach (var userId in added)
            {
                var user = validatedData?.Users?.TryGetNullable(userId) ?? await _databaseUserService.GetItem(new UserWithId(userId), cancellationToken);

                if (user != null && user.Applications.RemoveIfExists(x => x.Name == applicationAccess.Name))
                {
                    await _databaseUserService.PatchItem(user, cancellationToken);
                }
            }
        }

        private async Task UpdateGroups(RelatedData? validatedData, ApplicationWithId? before, ApplicationWithId? after, CancellationToken cancellationToken = default)
        {
            if (before == null && after == null) return;

            string id = (before?.GetId() ?? after?.GetId())!;
            var (removed, added, common) = before != null ? before.Groups.Diff(after?.Groups) : ([], after?.Groups ?? [], []);

            var applicationAccess = new ApplicationAccess() { Name = id };

            foreach (var groupId in removed ?? [])
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && group.Applications.AddIfNotExists(applicationAccess, x => x.Name == applicationAccess.Name))
                {
                    await _databaseGroupService.PatchItem(group, cancellationToken);
                }
            }


            foreach (var groupId in added)
            {
                var group = validatedData?.Groups?.TryGetNullable(groupId) ?? await _databaseGroupService.GetItem(new GroupWithId(groupId), cancellationToken);

                if (group != null && group.Applications.RemoveIfExists(x => x.Name == applicationAccess.Name))
                {
                    await _databaseGroupService.PatchItem(group, cancellationToken);
                }
            }
        }
    }
}
