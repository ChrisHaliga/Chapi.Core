using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.CrudServices
{
    public sealed class ApplicationService : DatabaseItemService<ApplicationWithId>
    {
        public ApplicationService(CrudConfigData<ApplicationWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo) {}
    }
}