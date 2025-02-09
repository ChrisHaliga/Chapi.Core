using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;

namespace Chapi.Api.Services.CrudServices
{

    public abstract class CrudServiceBase<T> where T : IDatabaseItemWithId
    {
        private readonly IDatabaseService _databaseService;

        public CrudServiceBase(CrudConfigData<T> crudConfig, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo)
        {
            _databaseService = new CosmosDatabaseService(cosmosConfig, cache, runtimeInfo, crudConfig.DatabaseName, crudConfig.ContainerName);
        }

        internal virtual async Task<T> GetItem(T item, CancellationToken cancellationToken = default)
        {
            var foundItem = await _databaseService.GetItemByIdAsync<T>(item.GetId(), cancellationToken);
            
            if(foundItem == null)
            {
                throw new NotFoundException(item);
            }

            return foundItem;
        }

        internal virtual async Task<List<T>> GetItemsWhereKeyIsValue(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default)
        {
            var foundItems = await _databaseService.ListItemsAsync<T>(keyValuePair, cancellationToken);

            if (foundItems == null || foundItems.Count == 0)
            {
                throw new NotFoundException();
            }

            return foundItems;
        }

        internal virtual async Task<List<T>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var foundItems = await _databaseService.ListAllItemsAsync<T>( cancellationToken);

            if (foundItems == null || foundItems.Count == 0)
            {
                throw new NotFoundException();
            }

            return foundItems;
        }

        internal virtual async Task<T> CreateItem(T item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKey()))
            {
                throw new BadRequestException(item);
            }

            return await _databaseService.CreateItemAsync(item, cancellationToken);
        }

        internal virtual async Task<T> UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKey()))
            {
                throw new BadRequestException(item);
            }

            return await _databaseService.UpdateItemAsync(item, hard, cancellationToken);
        }

        internal virtual async Task DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync(item, cancellationToken);
        }
    }
}
