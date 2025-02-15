using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.CrudServices
{
    public sealed class UserService : DatabaseItemService<UserWithId>
    {
        public UserService(CrudConfigData<UserWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo){}
    }
}
