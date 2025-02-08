using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services.Database;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<User, UserWithId>
    {
        private readonly GroupService _groupService;
        public UserService(CrudConfigData<User> config, CosmosConfigData cosmosConfig, CacheService cache, RuntimeInfo runtimeInfo, GroupService groupService) : base(config, cosmosConfig, cache, runtimeInfo)
        {
            _groupService = groupService;
        }

        public async Task<User> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var userQuery = new User() { Email = email };

            return await GetItem(userQuery, cancellationToken);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken)
        {
            var keyValuePair = new KeyValuePair<string, string>("organization", organization);
            return await GetItemsWhereKeyIsValue(keyValuePair, cancellationToken);
        }

        public async Task DeleteUser(UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            await DeleteItem(userMinimal.ToUser(), cancellationToken);
        }

        internal override async Task<User> CreateItem(User item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Organization))
            {
                throw new ArgumentException("Item must have an Id and a partition key");
            }

            var existingGroup = await _groupService.GetGroupByName(item.Organization, cancellationToken);

            return await CreateItem(item, cancellationToken);
        }
    }
}
