using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Chapi.Api.Models;
using Chapi.Api.Services;
using System.Text.Json;

namespace Chapi.Api.Wrappers
{
    internal class CosmosWrapper
    {
        private readonly string _databaseName;
        private readonly string _containerName;
        private readonly CacheService _cache;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosWrapper(string databaseName, string containerName, string CosmosDbUri, CacheService cache)
        {
            _databaseName = databaseName;
            _containerName = containerName;
            _cosmosClient = new CosmosClient(CosmosDbUri, new DefaultAzureCredential(), new CosmosClientOptions() { SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } });
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            _cache = cache;
        }

        internal async Task CreateItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var response = await _container.CreateItemAsync(item.ToCosmosItemWithId(), item.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Create(item.GetCacheKey(_databaseName, _containerName), item);
        }

        internal async Task<T?> GetItemAsync<T, TWithId>(T item, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cacheKey = item.GetCacheKey(_databaseName, _containerName);

            var cached = await _cache.Get<T>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var foundItem = !string.IsNullOrEmpty(item.GetId()) && item.GetPartitionKey() != PartitionKey.None
                ? await _container.ReadItemAsync<TWithId>(item.GetId(), item.GetPartitionKey(), cancellationToken: cancellationToken)
                : await GetItemWithQueryAsync<T, TWithId>(query ?? DefaultQueryDefinition(item.GetId()), cancellationToken);

            if (foundItem != null)
            {
                item.FromCosmosItemWithId(foundItem);
                await _cache.Create(cacheKey, item);
                return item;
            }

            return null;
        }

        internal async Task UpdateItemAsync<T, TWithId>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changes = item.CreateInstance();
            CosmosItem<TWithId>.InjectValues(item, changes);

            var cacheKey = item.GetCacheKey(_databaseName, _containerName);

            var existing = await GetItemAsync<T, TWithId>(item, cancellationToken: cancellationToken);

            if (existing == null)
            {
                await CreateItemAsync<CosmosItem<TWithId>, TWithId>(item, cancellationToken);
                return;
            }

            CosmosItem<TWithId>.InjectValues(changes, existing, hard);

            await _container.UpsertItemAsync(existing.ToCosmosItemWithId(), existing.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Create(cacheKey, existing);
        }

        internal async Task DeleteItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            cancellationToken.ThrowIfCancellationRequested();

            await _container.DeleteItemAsync<T>(item.GetId(), item.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Remove(item.GetCacheKey(_databaseName, _containerName));
        }

        private QueryDefinition DefaultQueryDefinition(string id) => new QueryDefinition("SELECT * FROM c WHERE c.id = @Id").WithParameter("@Id", id);


        private async Task<TWithId?> GetItemWithQueryAsync<T, TWithId>(QueryDefinition query, CancellationToken cancellationToken) where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
        {
            using var feedIterator = _container.GetItemQueryIterator<TWithId>(query);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);

                if (response.Resource.Any())
                {
                    return response.Resource.FirstOrDefault();
                }
            }

            return default;
        }
    }
}
