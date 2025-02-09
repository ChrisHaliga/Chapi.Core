using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<UserWithId>
    {
        private readonly GroupService _groupService;
        public UserService(CrudConfigData<UserWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo, GroupService groupService) : base(config, cosmosConfig, cache, runtimeInfo)
        {
            _groupService = groupService;
        }

        public async Task<User> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var userQuery = new User() { Email = email }.ToUserWithId();

            return await GetItem(userQuery, cancellationToken);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken)
        {
            var keyValuePair = new KeyValuePair<string, string>("organization", organization);
            return await GetItemsWhereKeyIsValue(keyValuePair, cancellationToken);
        }

        public async Task DeleteUser(UserWithId userMinimal, CancellationToken cancellationToken)
        {
            await DeleteItem(userMinimal, cancellationToken);
        }

        internal override async Task<UserWithId> CreateItem(UserWithId item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.Organization))
            {
                throw new BadRequestException(item);
            }

            var existingGroup = await _groupService.GetGroupByName(item.Organization, cancellationToken);

            return await _CreateItem(item, cancellationToken);
        }
    }
}
