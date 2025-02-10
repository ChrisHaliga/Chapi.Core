using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using System.Threading;

namespace Chapi.Api.Services.CrudServices
{
    public class GroupService : CrudServiceBase<GroupWithId>
    {
        private GroupService _groupService => CrudServiceBroker.GroupService ?? throw new Exception("Broker Service was not ready");
        private ApplicationService _applicationService => CrudServiceBroker.ApplicationService ?? throw new Exception("Broker Service was not ready");
        private UserService _userService => CrudServiceBroker.UserService ?? throw new Exception("Broker Service was not ready");

        public GroupService(CrudConfigData<GroupWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo)
        {
            CrudServiceBroker.GroupService = this;
        }

        #region Virtual Overrides

        public override async Task<GroupWithId> GetItemById(string name, CancellationToken cancellationToken = default) =>  
            await _GetItem(new GroupWithId(new Group() { Name = name }), cancellationToken);

        public override async Task<List<GroupWithId>> GetItemsByPartitionKey(string parent, CancellationToken cancellationToken = default) => 
            await _GetItemsWhereKeyIsValue(new KeyValuePair<string, string>(nameof(Group.Parent).ToLower(), parent), cancellationToken);

#endregion

#region Abstract Overrides

        public override async Task<GroupWithId> CreateItem(GroupWithId group, CancellationToken cancellationToken = default)
        {
            List<ApplicationWithId> exitingApplications = await GetExistingApplications(group, cancellationToken);

            var createdGroup = await CreateItem(group, cancellationToken);

            foreach (var application in exitingApplications)
            {
                await _applicationService.AssignGroupInApplication(group.GetId(), application);
            }

            return createdGroup;
        }

        public override async Task DeleteItem(GroupWithId group, CancellationToken cancellationToken = default)
        {
            await UnassignGroupInApplications(group, cancellationToken);
            await UnassignGroupInUsers(group, cancellationToken);

            await _DeleteItem(group, cancellationToken);
        }

        #endregion

        private async Task<List<ApplicationWithId>> GetExistingApplications(GroupWithId group, CancellationToken cancellationToken)
        {
            var exitingApplications = new List<ApplicationWithId>();
            foreach (var application in group.Applications)
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

        public async Task<GroupWithId> AssignUserToGroup(UserWithId user, GroupWithId group, CancellationToken cancellationToken = default) => 
            await ChangeUserAssignmentToGroup(user, group, true, cancellationToken);
        public async Task<GroupWithId> UnassignUserToGroup(UserWithId user, GroupWithId group, CancellationToken cancellationToken = default) =>
            await ChangeUserAssignmentToGroup(user, group, false, cancellationToken);

        private async Task<GroupWithId> ChangeUserAssignmentToGroup(UserWithId user, GroupWithId group, bool assignment, CancellationToken cancellationToken = default)
        {
            if ((group.Members.TryGetValue(user.GetId(), out bool isMember) || isMember) != assignment)
            {
                group.Members[user.GetId()] = assignment;

                return await PatchItem(group, cancellationToken);
            }

            return group;
        }

        private async Task UnassignGroupInUsers(GroupWithId group, CancellationToken cancellationToken)
        {
            foreach (var user in group.Members)
            {
                if (!string.IsNullOrEmpty(user.Key))
                {
                    try
                    {
                        var existingUser = await _userService.GetItemById(user.Key, cancellationToken);
                        await _userService.UnassignGroupInUser(group.GetId(), existingUser);
                    }
                    catch (NotFoundException) { }
                }
            }
        }

        private async Task UnassignGroupInApplications(GroupWithId group, CancellationToken cancellationToken)
        {
            foreach (var application in group.Applications)
            {
                if (!string.IsNullOrEmpty(application.Name))
                {
                    try
                    {
                        var existingApplication = await _applicationService.GetItemById(application.Name, cancellationToken);
                        await _applicationService.UnassignGroupInApplication(group.GetId(), existingApplication);
                    }
                    catch (NotFoundException) { }
                }
            }
        }
    }
}
