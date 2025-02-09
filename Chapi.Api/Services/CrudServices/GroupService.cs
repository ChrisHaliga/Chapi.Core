using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Services.CrudServices
{
    public class GroupService : CrudServiceBase<GroupWithId>
    {
        public GroupService(CrudConfigData<GroupWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) { }

        public async Task<GroupWithId> GetGroupByName(string name, CancellationToken cancellationToken)
        {
            var group = new Group() { Name = name }.ToGroupWithId();

            var foundGroup = await GetItem(group, cancellationToken);

            if(foundGroup == null)
            {
                throw new NotFoundException(group);
            }

            return foundGroup;
        }

        public async Task<List<GroupWithId>> GetGroupsByParent(string parent, CancellationToken cancellationToken)
        {
            return await GetItemsWhereKeyIsValue(new KeyValuePair<string, string>("parent", parent), cancellationToken);
        }

        public async Task DeleteGroup(GroupWithId group, CancellationToken cancellationToken)
        {
            await DeleteItem(group, cancellationToken);
        }

        internal override async Task<GroupWithId> CreateItem(GroupWithId item, CancellationToken cancellationToken = default)
        {
            return await _CreateItem(item, cancellationToken);
        }
    }
}
