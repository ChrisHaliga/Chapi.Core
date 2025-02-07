using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services.CrudServices
{

    public abstract class CrudServiceBase<T, TWithId> where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _databaseName;
        private readonly string _containerName;

        public CrudServiceBase(IDatabaseService cosmosService, CrudConfigData<T> config)
        {
            _databaseService = cosmosService;
            _databaseName = config.DatabaseName;
            _containerName = config.ContainerName;
        }

        internal virtual async Task<RequestDetailObject> GetItem(T item, CancellationToken cancellationToken = default)
        {
            var response = await _databaseService.GetItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken: cancellationToken);
            
            if(response == null)
            {
                return RequestDetails.NotFound();
            }

            return RequestDetails.Success(response);
        }

        internal virtual async Task<List<TWithId>> GetItems(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemsAsync<T, TWithId>(_databaseName, _containerName, query, cancellationToken);
        }

        internal virtual async Task<List<TWithId>> GetItemsByPartitionKey(string partitionKeyName, string partitionKeyValue, CancellationToken cancellationToken = default)
        {
            return await GetItems(new QueryDefinition($"SELECT * FROM c WHERE c.{partitionKeyName} = @partitionKey").WithParameter("@partitionKey", partitionKeyValue), cancellationToken);
        }

        internal virtual async Task<List<TWithId>> GetAllItems(CancellationToken cancellationToken = default)
        {
            return await GetItems(new QueryDefinition("SELECT * FROM c"), cancellationToken);
        }

        internal virtual async Task CreateItem(T item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKeyString()))
            {
                throw new ArgumentException("Item must have an Id and a partition key");
            }

            await _databaseService.CreateItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }

        internal virtual async Task UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKeyString()))
            {
                throw new ArgumentException("Item must have an Id and a partition key");
            }

            await _databaseService.UpdateItemAsync<T, TWithId>(item, _databaseName, _containerName, hard, cancellationToken);
        }

        internal virtual async Task DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }
    }
}
