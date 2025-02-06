using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Users;

namespace Chapi.Api.Services
{
    public class UsersService: ICosmosCrudConsumer<User, UserDto>
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

            await _databaseService.CreateItemAsync<User, UserDto>(item, _databaseName, _containerName, cancellationToken);
        }

        public async Task<UserDto?> GetAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemAsync<User, UserDto>(new User(new UserDto() { Email = email }), _databaseName, _containerName, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user?.Email == null) throw new ArgumentNullException(nameof(user.Email));

            await _databaseService.UpdateItemAsync<User, UserDto>(user, _databaseName, _containerName, cancellationToken);
        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user?.Email == null) throw new ArgumentNullException(nameof(user.Email));
            if (user?.Organization == null) throw new ArgumentNullException(nameof(user.Organization));

            await _databaseService.DeleteItemAsync<User, UserDto>(user, _databaseName, _containerName, cancellationToken);
        }


    }
}
