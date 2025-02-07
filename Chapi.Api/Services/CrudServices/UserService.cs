using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<User, UserWithId>
    {
        private readonly GroupService _groupService;
        public UserService(CrudConfigData<User> config, IDatabaseService cosmosService, GroupService groupService) : base(cosmosService, config)
        {
            _groupService = groupService;
        }

        public async Task<RequestDetailObject> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var userQuery = new User() { Email = email };

            return await GetItem(userQuery, cancellationToken);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken)
        {
            return await GetItemsByPartitionKey("organization", organization, cancellationToken);
        }

        public async Task DeleteUser(UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            await DeleteItem(userMinimal.ToUser(), cancellationToken);
        }

        internal override async Task<RequestDetailObject> CreateItem(User item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Organization))
            {
                throw new ArgumentException("Item must have an Id and a partition key");
            }

            var existingGroup = await _groupService.GetGroupByName(item.Organization, cancellationToken);

            if(existingGroup.Status != RequestStatus.Success)
            {
                return existingGroup;
            }

            await CreateItem(item, cancellationToken);

            return RequestDetails.Success(item);
        }
    }
}
