using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;
using System.Collections.Generic;

namespace Chapi.Api.Services.CrudServices
{

    public abstract class CrudServiceBase<T, TWithId> where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
    {
        private readonly IDatabaseService _databaseService;

        public CrudServiceBase(CrudConfigData<T> crudConfig, CosmosConfigData cosmosConfig, CacheService cache, RuntimeInfo runtimeInfo)
        {
            _databaseService = new CosmosDatabaseService(cosmosConfig, cache, runtimeInfo, crudConfig.DatabaseName, crudConfig.ContainerName);        }

        internal virtual async Task<T> GetItem(T item, CancellationToken cancellationToken = default)
        {
            var foundItem = await _databaseService.GetItemByIdAsync<T, TWithId>(item, cancellationToken: cancellationToken);
            
            if(foundItem == null)
            {
                throw new NotFoundException((DatabaseItem<DatabaseItemWithId>)(object)item);
            }

            return foundItem;
        }

        internal virtual async Task<List<TWithId>> GetItemsWhereKeyIsValue(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default)
        {
            var foundItems = await _databaseService.GetItemsAsync<T, TWithId>(keyValuePair, cancellationToken);

            if (foundItems == null || foundItems.Count == 0)
            {
                throw new NotFoundException();
            }

            return foundItems;
        }

        internal virtual async Task<List<TWithId>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var foundItems = await _databaseService.GetAllItemsAsync<T, TWithId>( cancellationToken);

            if (foundItems == null || foundItems.Count == 0)
            {
                throw new NotFoundException();
            }

            return foundItems;
        }

        internal virtual async Task<T> CreateItem(T item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKeyString()))
            {
                throw new BadRequestException((DatabaseItem<DatabaseItemWithId>)(object)item);
            }

            return await _databaseService.CreateItemAsync<T, TWithId>(item, cancellationToken);
        }

        internal virtual async Task<T> UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKeyString()))
            {
                throw new BadRequestException((DatabaseItem<DatabaseItemWithId>)(object)item);
            }

            return await _databaseService.UpdateItemAsync<T, TWithId>(item, hard, cancellationToken);
        }

        internal virtual async Task DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync<T, TWithId>(item, cancellationToken);
        }
    }
}
