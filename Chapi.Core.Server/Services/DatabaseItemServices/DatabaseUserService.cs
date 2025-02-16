using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.DatabaseItemServices
{
    public sealed class DatabaseUserService : DatabaseItemService<UserWithId>
    {
        public DatabaseUserService(CrudConfigData<UserWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo){}
    }
}
