using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.DatabaseItemServices
{
    public sealed class DatabaseApplicationService : DatabaseItemService<ApplicationWithId>
    {
        public DatabaseApplicationService(CrudConfigData<ApplicationWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) {}
    }
}