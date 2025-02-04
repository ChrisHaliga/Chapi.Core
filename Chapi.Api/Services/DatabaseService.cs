using Azure.Identity;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Wrappers;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services
{
    public interface IDatabaseService
    {
        Task CreateItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase;
        Task<T?> GetItemAsync<T>(string id, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase;
        Task UpdateItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase;
        Task DeleteItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase;
    }

    internal class DatabaseService: IDatabaseService
    {
        private readonly CacheService _cache;
        private readonly string _cosmosDbUri;
        private CosmosWrapper CosmosService(string databaseName, string containerName) => new CosmosWrapper(databaseName, containerName, _cosmosDbUri, _cache);

        public DatabaseService(CosmosConfigData config, CacheService cache)
        {
            _cache = cache;
            _cosmosDbUri = config.CosmosDbUri;
        }

        public async Task CreateItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            await CosmosService(databaseName, containerName).CreateItemAsync(new CosmosItemData<T>(item, partitionKey), cancellationToken);
        }

        public async Task<T?> GetItemAsync<T>(string id, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            return await CosmosService(databaseName, containerName).GetItemAsync(new CosmosItemData<T>(id), query, cancellationToken);
        }


        public async Task UpdateItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            await CosmosService(databaseName, containerName).UpdateItemAsync(new CosmosItemData<T>(item, partitionKey), cancellationToken);
        }

        public async Task DeleteItemAsync<T>(T item, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            await CosmosService(databaseName, containerName).DeleteItemAsync(new CosmosItemData<T>(item, partitionKey), cancellationToken);
        }

        public async Task DeleteItemAsync<T>(string id, string databaseName, string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            await CosmosService(databaseName, containerName).DeleteItemAsync(new CosmosItemData<T>(id, partitionKey), cancellationToken);
        }
    }
}
