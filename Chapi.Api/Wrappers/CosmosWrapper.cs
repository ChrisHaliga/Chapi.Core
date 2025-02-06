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

        internal async Task CreateItemAsync<T, Dto>(T item, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var response = await _container.CreateItemAsync(item, item.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Create(item.CacheKey(_databaseName, _containerName), item);
        }

        internal async Task<Dto?> GetItemAsync<T, Dto>(T item, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cacheKey = item.CacheKey(_databaseName, _containerName);

            var cached = await _cache.Get<Dto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var foundItem = !string.IsNullOrEmpty(item.Id) && item.PartitionKey != PartitionKey.None
                ? await _container.ReadItemAsync<Dto>(item.Id, item.PartitionKey, cancellationToken: cancellationToken)
                : await GetItemWithQueryAsync<T, Dto>(query ?? DefaultQueryDefinition(item.Id), cancellationToken);

            if (foundItem != null)
            {
                await _cache.Create(cacheKey, foundItem);
            }

            return foundItem;
        }

        internal async Task UpdateItemAsync<T, Dto>(T item, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = item.CacheKey(_databaseName, _containerName);

            var existing = await GetItemAsync<T, Dto>(item, cancellationToken: cancellationToken);

            if (existing == null)
            {
                await CreateItemAsync<T, Dto>(item, cancellationToken);
                return;
            }

            OverrideWithNonNullValues(existing, item);

            await _container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Create(cacheKey, item);
        }

        internal async Task DeleteItemAsync<T, Dto>(T item, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            cancellationToken.ThrowIfCancellationRequested();

            await _container.DeleteItemAsync<T>(item.Id, item.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Remove(item.CacheKey(_databaseName, _containerName));
        }

        private QueryDefinition DefaultQueryDefinition(string id) => new QueryDefinition("SELECT * FROM c WHERE c.id = @Id").WithParameter("@Id", id);


        private async Task<Dto?> GetItemWithQueryAsync<T, Dto>(QueryDefinition query, CancellationToken cancellationToken)
        {
            using var feedIterator = _container.GetItemQueryIterator<Dto>(query);

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

        private void OverrideWithNonNullValues<T, Dto>(Dto baseItem, T overridingItem)
        {
            if (overridingItem == null)
            {
                return;
            }

            foreach (var prop in typeof(T).GetProperties())
            {
                var overridingValue = prop.GetValue(overridingItem);
                if (overridingValue != null)
                {
                    prop.SetValue(baseItem, overridingValue);
                }
            }
        }
    }
}
