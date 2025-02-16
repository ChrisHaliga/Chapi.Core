using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.DatabaseItemServices;

namespace Chapi.Api.Services.ApiServices
{
    public abstract partial class ApiServiceBase<T> : IApiService<T> where T : IDatabaseItemWithId
    {
        protected readonly DatabaseUserService _databaseUserService;
        protected readonly DatabaseGroupService _databaseGroupService;
        protected readonly DatabaseApplicationService _databaseApplicationService;

        protected ApiServiceBase(DatabaseUserService databaseUserService, DatabaseGroupService databaseGroupService, DatabaseApplicationService databaseApplicationService)
        {
            _databaseUserService = databaseUserService;
            _databaseGroupService = databaseGroupService;
            _databaseApplicationService = databaseApplicationService;
        }


        protected abstract IDatabaseItemService<T> PrimaryDatabaseService { get; }
        protected abstract T CopyPrimaryDatabaseItemWithId(T item);
        protected abstract void SoftOverwriteWith(T target, T source);

        public abstract Task<T> MigrateItem(T item, string newPartition, CancellationToken cancellationToken = default);
        public abstract Task<RelatedData> ValidateItem(T item, CancellationToken cancellationToken = default);
        public abstract Task UpdateRelatedItems(RelatedData? validatedData, T? before, T? after, CancellationToken cancellationToken = default);

        public virtual async Task<T> GetItem(T item, CancellationToken cancellationToken = default)
        {
            return await PrimaryDatabaseService.GetItem(item, cancellationToken);
        }

        public virtual async Task<List<T>> GetItemByPartition(string parent, CancellationToken cancellationToken = default)
        {
            return await PrimaryDatabaseService.GetItemsWhereKeyIsValue(nameof(UserWithId.Organization), parent, cancellationToken);
        }

        public virtual async Task<List<T>> GetAllItems(CancellationToken cancellationToken = default)
        {
            return await PrimaryDatabaseService.GetAllItems(cancellationToken);
        }

        public virtual async Task<T> CreateItem(T item, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateItem(item, cancellationToken);

            await UpdateRelatedItems(validatedData, default, item, cancellationToken);

            return await PrimaryDatabaseService.CreateItem(item, cancellationToken);
        }

        public virtual async Task<T> PatchItem(T item, CancellationToken cancellationToken = default)
        {
            var existingItem = await PrimaryDatabaseService.GetItem(item, cancellationToken);
            var newItem = CopyPrimaryDatabaseItemWithId(existingItem);
            SoftOverwriteWith(item, newItem);

            var validatedData = await ValidateItem(newItem, cancellationToken);

            await UpdateRelatedItems(validatedData, existingItem, newItem, cancellationToken);

            return await PrimaryDatabaseService.UpdateItem(newItem, hard: false, cancellationToken);
        }

        public virtual async Task<T> PutItem(T item, CancellationToken cancellationToken = default)
        {
            var existingUser = await PrimaryDatabaseService.GetItem(item);
            var validatedData = await ValidateItem(item, cancellationToken);

            await UpdateRelatedItems(validatedData, existingUser, item, cancellationToken);

            return await PrimaryDatabaseService.UpdateItem(item, hard: true, cancellationToken);
        }

        public virtual async Task DeleteItem(T itemQuery, CancellationToken cancellationToken = default)
        {
            var item = await PrimaryDatabaseService.GetItem(itemQuery, cancellationToken);

            await UpdateRelatedItems(null, item, default, cancellationToken);
            await PrimaryDatabaseService.DeleteItem(item, cancellationToken);
        }

        internal async Task EnsureUsersExist(RelatedData result, IDatabaseItem item, List<string> userIds, CancellationToken cancellationToken)
        {
            foreach (var userId in userIds)
            {
                var existingUser = (await _databaseUserService.GetItemIfExists(new UserWithId(userId), cancellationToken)) ?? throw new NotFoundException(item, $"User \"{userId}\" does not exist");

                result.Users[existingUser.GetId()] = existingUser;
            }
        }

        internal async Task EnsureOrganizationExists(RelatedData result, IDatabaseItem item, string? organizationId, CancellationToken cancellationToken)
        {
            var itemAsGroup = item as GroupWithId;
            if (itemAsGroup?.IsOrganization() ?? false)
            {
                return;
            }

            var existingOrganization = (await _databaseGroupService.GetItemIfExists(new GroupWithId(organizationId, organizationId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{organizationId}\" does not exist");
            if (existingOrganization.Name != existingOrganization.Organization)
            {
                throw new NotFoundException(item, $"Group \"{organizationId}\" is not a organization");
            }
        }

        internal async Task EnsureParentExists(RelatedData result, IDatabaseItem item, string? parentId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return; //Having no parent is valid
            }

            var existingParent = (await _databaseGroupService.GetItem(new GroupWithId(parentId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{parentId}\" does not exist");
            if (existingParent == null)
            {
                throw new NotFoundException(item, $"Group \"{existingParent}\" is not an organization");
            }

            result.Groups[existingParent.GetId()] = existingParent;
        }

        internal async Task EnsureChildrenExist(RelatedData result, IDatabaseItem item, List<string> groupIds, CancellationToken cancellationToken)
        {
            foreach (var groupId in groupIds)
            {
                var existingGroup = (await _databaseGroupService.GetItemIfExists(new GroupWithId(groupId), cancellationToken)) ?? throw new NotFoundException(item, $"Group \"{groupId}\" does not exist");

                result.Groups[existingGroup.GetId()] = existingGroup;
            }
        }

        internal async Task EnsureApplicationsAndRolesExist(RelatedData result, IDatabaseItem item, List<ApplicationAccess> applicationAccesses, CancellationToken cancellationToken)
        {
            foreach (var applicationAccess in applicationAccesses)
            {
                var existingApplication = await _databaseApplicationService.GetItemIfExists(new ApplicationWithId(applicationAccess.Name), cancellationToken);

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
    }

    public class RelatedData //This is mostly just to save db calls when validating before taking action
    {
        public Dictionary<string, UserWithId> Users { get; set; } = new Dictionary<string, UserWithId>();
        public Dictionary<string, GroupWithId> Groups { get; } = new Dictionary<string, GroupWithId>();
        public Dictionary<string, ApplicationWithId> Applications { get; } = new Dictionary<string, ApplicationWithId>();
    }
}
