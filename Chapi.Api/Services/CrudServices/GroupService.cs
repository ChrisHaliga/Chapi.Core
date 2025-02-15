using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.CrudServices
{
    public sealed class GroupService : DatabaseItemService<GroupWithId>
    {
        public GroupService(CrudConfigData<GroupWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) { }
    }
}
