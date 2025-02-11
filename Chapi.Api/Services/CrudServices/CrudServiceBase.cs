using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.Database;

namespace Chapi.Api.Services.CrudServices
{

    public abstract class CrudServiceBase<T> : ICrudServiceBase<T> where T : IDatabaseItemWithId
    {
        private readonly IDatabaseService _databaseService;

        protected CrudServiceBase(CrudConfigData<T> crudConfig, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo)
        {
            _databaseService = new CosmosDatabaseService(cosmosConfig, cache, runtimeInfo, crudConfig.DatabaseName, crudConfig.ContainerName);
        }


        public abstract Task<T> GetItemById(string id, CancellationToken cancellationToken = default);
        public abstract Task<List<T>> GetItemsByPartitionKey(string partitionKey, CancellationToken cancellationToken = default);
        public virtual async Task<T> GetItem(T item, CancellationToken cancellationToken = default) => await _GetItem(item, cancellationToken);
        public virtual async Task<T?> GetItemIfExists(T item, CancellationToken cancellationToken = default) => await _GetItemIfExists(item, cancellationToken);
        public virtual async Task<List<T>> GetItemsWhereKeyIsValue(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default) => await _GetItemsWhereKeyIsValue(keyValuePair, cancellationToken);
        public virtual async Task<List<T>> GetAllItems(CancellationToken cancellationToken = default) => await _GetAllItems(cancellationToken);
        public virtual async Task<T> CreateItem(T item, CancellationToken cancellationToken = default) => await _CreateItem(item, cancellationToken);
        public virtual async Task<T> UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default) => await _UpdateItem(item, hard, cancellationToken);
        public virtual async Task<T> PutItem(T item, CancellationToken cancellationToken = default) => await _UpdateItem(item, true, cancellationToken);
        public virtual async Task<T> PatchItem(T item, CancellationToken cancellationToken = default) => await _UpdateItem(item, false, cancellationToken);
        public virtual async Task DeleteItem(T item, CancellationToken cancellationToken = default) => await _DeleteItem(item, cancellationToken);
        
        protected async Task<T> _GetItem(T item, CancellationToken cancellationToken = default)
        {
            var foundItem = await _databaseService.GetItemByIdAsync<T>(item.GetId(), cancellationToken);

            return foundItem ?? throw new NotFoundException(item);
        }

        protected async Task<T?> _GetItemIfExists(T item, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _GetItem(item, cancellationToken);
            }
            catch (NotFoundException)
            {
                return default;
            }
        }


        protected async Task<List<T>> _GetItemsWhereKeyIsValue(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default) =>
            await _databaseService.ListItemsAsync<T>(keyValuePair, cancellationToken) ?? throw new NotFoundException();

        protected async Task<List<T>> _GetAllItems(CancellationToken cancellationToken = default) => await _databaseService.ListAllItemsAsync<T>(cancellationToken) ?? throw new NotFoundException();

        protected async Task<T> _CreateItem(T item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKey()))
            {
                throw new BadRequestException(item, "DatabaseItems must have an Id and Partition Key");
            }

            return await _databaseService.CreateItemAsync(item, cancellationToken);
        }

        protected async Task<T> _UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKey()))
            {
                throw new BadRequestException(item, "DatabaseItems must have an Id and Partition Key");
            }

            return await _databaseService.UpdateItemAsync(item, hard, cancellationToken);
        }

        protected async Task _DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync(item, cancellationToken);
        }
    }
}
