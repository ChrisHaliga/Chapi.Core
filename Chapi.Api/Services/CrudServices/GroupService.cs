using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using System.Threading;

namespace Chapi.Api.Services.CrudServices
{
    public class GroupService : CrudServiceBase<GroupWithId>
    {
        public GroupService(CrudConfigData<GroupWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) { }

        public override async Task<GroupWithId> GetItemById(string name, CancellationToken cancellationToken = default) =>  
            await _GetItem(new GroupWithId(new Group() { Name = name }), cancellationToken);

        public override async Task<List<GroupWithId>> GetItemsByPartitionKey(string parent, CancellationToken cancellationToken = default) => 
            await _GetItemsWhereKeyIsValue(new KeyValuePair<string, string>(nameof(Group.Parent).ToLower(), parent), cancellationToken);
    }
}
