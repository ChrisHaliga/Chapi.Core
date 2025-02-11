using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<UserWithId>
    {
        public UserService(CrudConfigData<UserWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo){}


        public override async Task<UserWithId> GetItemById(string email, CancellationToken cancellationToken = default) =>
            await _GetItem(new UserWithId(new User() { Email = email }), cancellationToken);

        public override async Task<List<UserWithId>> GetItemsByPartitionKey(string organization, CancellationToken cancellationToken = default) =>
            await _GetItemsWhereKeyIsValue(new KeyValuePair<string, string>(nameof(User.Organization).ToLower(), organization), cancellationToken);

    }
}
