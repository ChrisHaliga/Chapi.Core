using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Users;

namespace Chapi.Api.Services
{
    public class UsersService: ICosmosCrudConsumer<User>
    {
        IDatabaseService _databaseService;
        private readonly string _databaseName;
        private readonly string _containerName;
        public UsersService(UsersConfigData config, IDatabaseService cosmosService)
        {
            _databaseService = cosmosService;
            _databaseName = config.DatabaseName;
            _containerName = config.ContainerName;
        }

        public async Task CreateAsync(User item, CancellationToken cancellationToken = default)
        {
            if (item?.Email == null) throw new ArgumentNullException(nameof(item.Email));
            if (item?.Organization == null) throw new ArgumentNullException(nameof(item.Organization));

            await _databaseService.CreateItemAsync(item, _databaseName, _containerName, item.Organization, cancellationToken);
        }

        public async Task<User?> GetAsync(string email, CancellationToken cancellationToken = default)
        {
            var query = new Microsoft.Azure.Cosmos.QueryDefinition("SELECT * FROM c WHERE c.email = @Email").WithParameter("@Email", email);
            return await _databaseService.GetItemAsync<User>(email, _databaseName, _containerName, query, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user?.Email == null) throw new ArgumentNullException(nameof(user.Email));

            var existing = await GetAsync(user.Email, cancellationToken);
            
            if(existing == null)
            {
                await CreateAsync(user, cancellationToken);
                return;
            }

            if (user.Organization == null)
            {
                user.Organization = user.Organization ?? existing.Organization;
            }
            if (user.Name == null)
            {

            }
            if (user.ProfilePicture == null)
            {

            }
            if (user.Access == null)
            {

            }

            await _databaseService.UpdateItemAsync(user, _databaseName, _containerName, user.Organization, cancellationToken);
        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user?.Email == null) throw new ArgumentNullException(nameof(user.Email));
            if (user?.Organization == null) throw new ArgumentNullException(nameof(user.Organization));

            await _databaseService.DeleteItemAsync(user, _databaseName, _containerName, user.Organization, cancellationToken);
        }


    }
}
