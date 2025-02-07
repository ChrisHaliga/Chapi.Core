using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Services.CrudServices
{
    public class GroupService : CrudServiceBase<Group, GroupWithId>
    {
        public GroupService(CrudConfigData<Group> config, IDatabaseService cosmosService) : base(cosmosService, config) { }

        public async Task<Group?> GetGroupByName(string name, CancellationToken cancellationToken)
        {
            var user = new Group() { Name = name };
            return await GetItem(user, cancellationToken);
        }

        public async Task<List<GroupWithId>> GetGroupsByParent(string parent, CancellationToken cancellationToken)
        {
            return await GetItemsByPartitionKey("parent", parent, cancellationToken);
        }

        public async Task DeleteGroup(GroupMinimalDto group, CancellationToken cancellationToken)
        {
            await DeleteItem(group.ToGroup(), cancellationToken);
        }
    }
}
