using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.DatabaseItemServices
{
    public sealed class DatabaseGroupService : DatabaseItemService<GroupWithId>
    {
        public DatabaseGroupService(CrudConfigData<GroupWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) { }
    }
}
