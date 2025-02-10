using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<UserWithId>
    {
        private GroupService _groupService => CrudServiceBroker.GroupService ?? throw new Exception("Broker Service was not ready");
        private ApplicationService _applicationService => CrudServiceBroker.ApplicationService ?? throw new Exception("Broker Service was not ready");
        public UserService(CrudConfigData<UserWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo)
        {
            CrudServiceBroker.UserService = this;
        }

        #region Virtual Overrides

        public override async Task<UserWithId> GetItemById(string email, CancellationToken cancellationToken = default) =>
            await _GetItem(new UserWithId(new User() { Email = email }), cancellationToken);

        public override async Task<List<UserWithId>> GetItemsByPartitionKey(string organization, CancellationToken cancellationToken = default) =>
            await _GetItemsWhereKeyIsValue(new KeyValuePair<string, string>(nameof(User.Organization).ToLower(), organization), cancellationToken);

        #endregion

        #region Abstract Overrides

        public override async Task<UserWithId> CreateItem(UserWithId user, CancellationToken cancellationToken = default)
        {
            await AssignUserToOrganization(user, cancellationToken);
            await AssignUserToGroups(user, cancellationToken);

            await GetExistingApplications(user, cancellationToken); //Validates the application exists and the roles are valid

            return await _CreateItem(user, cancellationToken);
        }

        public override async Task DeleteItem(UserWithId user, CancellationToken cancellationToken = default)
        {
            await UnassignUserInOrganization(user, cancellationToken);
            await UnassignUserInGroups(user, cancellationToken);

            await _DeleteItem(user, cancellationToken);
        }

        #endregion

        public async Task<UserWithId> AssignGroupInUser(string id, UserWithId user, CancellationToken cancellationToken = default) =>
            await ChangeGroupInUser(id, user, true, cancellationToken);
        public async Task<UserWithId> UnassignGroupInUser(string id, UserWithId user, CancellationToken cancellationToken = default) =>
            await ChangeGroupInUser(id, user, false, cancellationToken);

        private async Task<UserWithId> ChangeGroupInUser(string id, UserWithId user, bool assignment, CancellationToken cancellationToken = default)
        {
            if (!assignment && user.Groups.Remove(id))
            {
                return await PatchItem(user, cancellationToken);
            }

            if (assignment && !user.Groups.Contains(id))
            {
                user.Groups.Add(id);
                return await PatchItem(user, cancellationToken);
            }

            return user;
        }

        public async Task<UserWithId> AssignApplicationInUser(ApplicationAccess applicationAccess, UserWithId user, CancellationToken cancellationToken = default) =>
            await ChangeApplicationInUser(applicationAccess, user, true, cancellationToken);
        public async Task<UserWithId> UnassignApplicationInUser(ApplicationAccess applicationAccess, UserWithId user, CancellationToken cancellationToken = default) =>
            await ChangeApplicationInUser(applicationAccess, user, false, cancellationToken);

        private async Task<UserWithId> ChangeApplicationInUser(ApplicationAccess applicationAccess, UserWithId user, bool assignment, CancellationToken cancellationToken = default)
        {
            var indexOfFoundApplication = user.Applications.FindIndex(x => x.Name == applicationAccess.Name);

            if (indexOfFoundApplication != -1 && !assignment)
            {
                user.Applications.RemoveAt(indexOfFoundApplication);
                return await PatchItem(user, cancellationToken);
            }

            if (indexOfFoundApplication == -1 && assignment)
            {
                user.Applications.Add(applicationAccess);
                return await PatchItem(user, cancellationToken);
            }

            if(indexOfFoundApplication != -1 && assignment)
            {
                user.Applications[indexOfFoundApplication] = applicationAccess;
                return await PatchItem(user, cancellationToken);
            }

            return user;
        }

        private async Task<List<ApplicationWithId>> GetExistingApplications(UserWithId user, CancellationToken cancellationToken)
        {
            var exitingApplications = new List<ApplicationWithId>();

            foreach (var application in user.Applications)
            {
                if (string.IsNullOrEmpty(application.Name))
                {
                    throw new BadRequestException($"{nameof(application.Name)} is required for {nameof(Application)}");
                }

                var existingApplication = await _applicationService.GetItemById(application.Name, cancellationToken);
                await _applicationService.ValidateRolesExist(application);

                exitingApplications.Add(existingApplication);
            }

            return exitingApplications;
        }

        private async Task AssignUserToOrganization(UserWithId user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.Organization))
            {
                throw new BadRequestException(user, "User must have an Organization");
            }

            var existingOrganization = await _groupService.GetItemById(user.Organization, cancellationToken);

            if (existingOrganization == null || existingOrganization.Parent != Group.OrganizationId)
            {
                throw new BadRequestException(user, "Organization must be a valid Organization");
            }

            await _groupService.AssignUserToGroup(user, existingOrganization);
        }

        private async Task AssignUserToGroups(UserWithId user, CancellationToken cancellationToken)
        {
            foreach (var group in user.Groups)
            {
                var existingGroup = await _groupService.GetItemById(group, cancellationToken);
                await _groupService.AssignUserToGroup(user, existingGroup, cancellationToken);
            }
        }

        private async Task UnassignUserInOrganization(UserWithId user, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(user.Organization))
            {
                try
                {
                    var existingGroup = await _groupService.GetItemById(user.Organization, cancellationToken);
                    await _groupService.UnassignUserToGroup(user, existingGroup, cancellationToken);
                }
                catch (NotFoundException) { }
            }
        }

        private async Task UnassignUserInGroups(UserWithId user, CancellationToken cancellationToken)
        {
            foreach (var group in user.Groups)
            {
                try
                {
                    var existingGroup = await _groupService.GetItemById(group, cancellationToken);
                    await _groupService.UnassignUserToGroup(user, existingGroup, cancellationToken);
                }
                catch (NotFoundException) { }
            }
        }
    }
}
