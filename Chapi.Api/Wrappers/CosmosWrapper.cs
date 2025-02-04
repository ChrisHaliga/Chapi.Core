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

        internal async Task CreateItemAsync<T>(CosmosItemData<T> itemData, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            if (itemData.Item == null) throw new ArgumentNullException(nameof(itemData.Item));

            cancellationToken.ThrowIfCancellationRequested();
            
            var response = await _container.CreateItemAsync(itemData.Item, itemData.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Create(itemData.CacheKey(_databaseName, _containerName), itemData.Item);
        }

        internal async Task<T?> GetItemAsync<T>(CosmosItemData<T> itemData, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cacheKey = itemData.CacheKey(_databaseName, _containerName);

            var cached = await _cache.Get<T>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var item = query != null 
                ? await GetItemWithQueryAsync<T>(query, cancellationToken)
                : await _container.ReadItemAsync<T>(itemData.Id, itemData.PartitionKey, cancellationToken: cancellationToken);

            if (item != null)
            {
                await _cache.Create(cacheKey, item);
            }

            return item;
        }

        internal async Task UpdateItemAsync<T>(CosmosItemData<T> itemData, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            if (itemData.Item == null) throw new ArgumentNullException(nameof(itemData.Item));

            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = itemData.CacheKey(_databaseName, _containerName);

            var existing = await GetItemAsync(itemData, cancellationToken: cancellationToken);

            if (existing == null)
            {
                await CreateItemAsync(itemData, cancellationToken);
                return;
            }

            OverrideWithNonNullValues(existing, itemData.Item);

            await _container.UpsertItemAsync(itemData.Item, itemData.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Create(cacheKey, itemData.Item);
        }

        internal async Task DeleteItemAsync<T>(CosmosItemData<T> itemData, CancellationToken cancellationToken = default) where T : CosmosDtoBase
        {
            if (itemData.Item == null) throw new ArgumentNullException(nameof(itemData.Item));

            cancellationToken.ThrowIfCancellationRequested();

            await _container.DeleteItemAsync<T>(itemData.Item.Id, itemData.PartitionKey, cancellationToken: cancellationToken);

            await _cache.Remove(itemData.CacheKey(_databaseName, _containerName));
        }

        private async Task<T?> GetItemWithQueryAsync<T>(QueryDefinition query, CancellationToken cancellationToken)
        {
            using var feedIterator = _container.GetItemQueryIterator<T>(query);

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

        private void OverrideWithNonNullValues<T>(T baseItem, T overridingItem)
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

    public class CosmosItemData<T> where T : CosmosDtoBase
    {
        public CosmosItemData(T item, string? partitionKey)
        {
            Id = item.Id;
            Item = item;
            PartitionKey = partitionKey == null ? new PartitionKey(partitionKey) : PartitionKey.None;
        }

        public CosmosItemData(string id, string? partitionKey = null)
        {
            Id = id;
            Item = default;
            PartitionKey = partitionKey == null ? new PartitionKey(partitionKey) : PartitionKey.None;
        }

        public string Id { get; }
        public PartitionKey PartitionKey { get; }
        public T? Item { get; }

        public string CacheKey(string databaseName, string containerName) => $"{databaseName}-{containerName}-{Id}";
    }
}
