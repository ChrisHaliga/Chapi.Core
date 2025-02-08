using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Services.CrudServices
{
    public class GroupService : CrudServiceBase<Group, GroupWithId>
    {
        public GroupService(CrudConfigData<Group> config, CosmosConfigData cosmosConfig, CacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) { }

        public async Task<Group> GetGroupByName(string name, CancellationToken cancellationToken)
        {
            var group = new Group() { Name = name };

            var foundGroup = await GetItem(group, cancellationToken);

            if(foundGroup == null)
            {
                throw new NotFoundException((DatabaseItem<DatabaseItemWithId>)(object)group);
            }

            return foundGroup;
        }

        public async Task<List<GroupWithId>> GetGroupsByParent(string parent, CancellationToken cancellationToken)
        {
            return await GetItemsWhereKeyIsValue(new KeyValuePair<string, string>("parent", parent), cancellationToken);
        }

        public async Task DeleteGroup(GroupMinimalDto group, CancellationToken cancellationToken)
        {
            await DeleteItem(group.ToGroup(), cancellationToken);
        }
    }
}
