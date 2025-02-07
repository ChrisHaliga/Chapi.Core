using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
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

        internal async Task<T?> GetItem(T item, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken: cancellationToken);
        }

        internal async Task<List<TWithId>> GetItems(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemsAsync<T, TWithId>(_databaseName, _containerName, query, cancellationToken);
        }

        internal async Task<List<TWithId>> GetItemsByPartitionKey(string partitionKeyName, string partitionKeyValue, CancellationToken cancellationToken = default)
        {
            return await GetItems(new QueryDefinition($"SELECT * FROM c WHERE c.{partitionKeyName} = @partitionKey").WithParameter("@partitionKey", partitionKeyValue), cancellationToken);
        }

        internal async Task<List<TWithId>> GetAllItems(CancellationToken cancellationToken = default)
        {
            return await GetItems(new QueryDefinition("SELECT * FROM c"), cancellationToken);
        }

        internal async Task CreateItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.CreateItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }

        internal async Task UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            await _databaseService.UpdateItemAsync<T, TWithId>(item, _databaseName, _containerName, hard, cancellationToken);
        }

        internal async Task DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }
    }
}
