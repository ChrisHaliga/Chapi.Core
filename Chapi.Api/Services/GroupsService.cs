using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Groups;
using Chapi.Api.Models.Users;

namespace Chapi.Api.Services
{
    public class GroupsService: ICosmosCrudConsumer<Group, GroupDto>
    {
        IDatabaseService _databaseService;
        private readonly string _databaseName;
        private readonly string _containerName;
        public GroupsService(GroupsConfigData config, IDatabaseService cosmosService)
        {
            _databaseService = cosmosService;
            _databaseName = config.DatabaseName;
            _containerName = config.ContainerName;
        }

        public async Task CreateAsync(Group item, CancellationToken cancellationToken = default)
        {
            await _databaseService.CreateItemAsync<Group, GroupDto>(item, _databaseName, _containerName, cancellationToken);
        }

        public async Task<GroupDto?> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemAsync<Group, GroupDto>(new Group(new GroupDto() { Name = name }), _databaseName, _containerName, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Group user, CancellationToken cancellationToken = default)
        {
            await _databaseService.UpdateItemAsync<Group, GroupDto>(user, _databaseName, _containerName, cancellationToken);
        }

        public async Task DeleteAsync(Group user, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync<Group, GroupDto>(user, _databaseName, _containerName, cancellationToken);
        }


    }
}
