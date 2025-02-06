using Azure.Identity;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Wrappers;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services
{
    public interface IDatabaseService
    {
        Task CreateItemAsync<T, TWithId>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId;
        Task<T?> GetItemAsync<T, TWithId>(T item, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId;
        Task UpdateItemAsync<T, TWithId>(T item, string databaseName, string containerName, bool hard = false, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId;
        Task DeleteItemAsync<T, TWithId>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId;
    }

    internal class DatabaseService : IDatabaseService
    {
        private readonly CacheService _cache;
        private readonly string _cosmosDbUri;

        private CosmosWrapper CosmosService(string databaseName, string containerName) => new CosmosWrapper(databaseName, containerName, _cosmosDbUri, _cache);

        public DatabaseService(CosmosConfigData config, CacheService cache)
        {
            _cache = cache;
            _cosmosDbUri = config.CosmosDbUri;
        }

        public async Task CreateItemAsync<T, TWithId>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            await CosmosService(databaseName, containerName).CreateItemAsync<T, TWithId>(item, cancellationToken);
        }

        public async Task<T?> GetItemAsync<T, TWithId>(T item, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            return await CosmosService(databaseName, containerName).GetItemAsync<T, TWithId>(item, query, cancellationToken);
        }


        public async Task UpdateItemAsync<T, TWithId>(T item, string databaseName, string containerName, bool hard = false, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
{
            await CosmosService(databaseName, containerName).UpdateItemAsync<T, TWithId>(item, hard, cancellationToken);
        }

        public async Task DeleteItemAsync<T, TWithId>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
{
            await CosmosService(databaseName, containerName).DeleteItemAsync<T, TWithId>(item, cancellationToken);
        }
    }
}
