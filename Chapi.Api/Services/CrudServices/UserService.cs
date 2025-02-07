using Chapi.Api.Models.Configuration;
using Chapi.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Services.CrudServices
{
    public class UserService : CrudServiceBase<User, UserWithId>
    {
        public UserService(CrudConfigData<User> config, IDatabaseService cosmosService) : base(cosmosService, config) { }

        public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var userQuery = new User() { Email = email };

            return await GetItem(userQuery);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken)
        {
            return await GetItemsByPartitionKey("organization", organization, cancellationToken);
        }

        public async Task DeleteUser(UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            await DeleteItem(userMinimal.ToUser(), cancellationToken);
        }
    }
}
