using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Chapi.Api.Models;
using Chapi.Api.Services;
using System.Text.Json;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Models.Configuration;

namespace Chapi.Api.Wrappers
{
    internal class CosmosWrapper
    {
        private readonly string _databaseName;
        private readonly string _containerName;
        private readonly CacheService _cache;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly RuntimeInfo _runtimeInfo;

        public CosmosWrapper(string databaseName, string containerName, string CosmosDbUri, CacheService cache, RuntimeInfo runtimeInfo)
        {
            _databaseName = databaseName;
            _containerName = containerName;
            _cosmosClient = new CosmosClient(CosmosDbUri, new DefaultAzureCredential(), new CosmosClientOptions() { SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } });
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            _cache = cache;
            _runtimeInfo = runtimeInfo;
        }

        internal async Task<T> CreateItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var response = await _container.CreateItemAsync(item.ToCosmosItemWithId(), item.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Create(item.GetCacheKey(_databaseName, _containerName), item);

            return item;
        }

        internal async Task<T?> GetItemByIdAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = item.GetId();

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException((DatabaseItem<DatabaseItemWithId>)(object)item);
            }

            var cacheKey = item.GetCacheKey(_databaseName, _containerName);
            var cached = await _cache.Get<T>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var foundItem = await _container.ReadItemAsync<TWithId>(id, item.GetPartitionKey(), cancellationToken: cancellationToken);

            if (foundItem != null)
            {
                item.FromCosmosItemWithId(foundItem);
                await _cache.Create(cacheKey, item);
                return item;
            }

            return null;
        }


        internal async Task<T?> GetItemAsync<T, TWithId>(QueryDefinition query, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var foundItem = await GetItemWithQueryAsync<T, TWithId>(query, cancellationToken);

                if (foundItem != null)
                {
                    T item = (T)(object)foundItem; // fix this later

                    var cacheKey = item.GetCacheKey(_databaseName, _containerName);

                    item.FromCosmosItemWithId(foundItem);
                    await _cache.Create(cacheKey, item);
                    return item;
                }
            }
            catch(Exception e) //Todo: figure out what exception this is and catch that instead
            {
                if (_runtimeInfo.IsDevelopment)
                {
                    throw new NotFoundException(e);
                }

                throw new NotFoundException();
            }

            return null;
        }

        internal async Task<List<TWithId>> GetItemsAsync<T, TWithId>(QueryDefinition query, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var feedIterator = _container.GetItemQueryIterator<TWithId>(query);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);

                if (response.Resource.Any())
                {
                    return response.Resource.ToList();
                }
            }

            return new List<TWithId>();
        }

        internal async Task<T> UpdateItemAsync<T, TWithId>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            var changes = item.CreateInstance();
            DatabaseItem<TWithId>.InjectValues(item, changes);

            var cacheKey = item.GetCacheKey(_databaseName, _containerName);

            var existing = await GetItemByIdAsync<T, TWithId>(item, cancellationToken: cancellationToken);

            if (existing == null)
            {
                await CreateItemAsync<DatabaseItem<TWithId>, TWithId>(item, cancellationToken);
                return item;
            }

            DatabaseItem<TWithId>.InjectValues(changes, existing, hard);

            await _container.UpsertItemAsync(existing.ToCosmosItemWithId(), existing.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Create(cacheKey, existing);
            return existing;
        }

        internal async Task DeleteItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            cancellationToken.ThrowIfCancellationRequested();

            await _container.DeleteItemAsync<T>(item.GetId(), item.GetPartitionKey(), cancellationToken: cancellationToken);

            await _cache.Remove(item.GetCacheKey(_databaseName, _containerName));
        }

        private QueryDefinition DefaultQueryDefinition(string id) => new QueryDefinition("SELECT * FROM c WHERE c.id = @Id").WithParameter("@Id", id);

        private async Task<TWithId?> GetItemWithQueryAsync<T, TWithId>(QueryDefinition query, CancellationToken cancellationToken) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            return (await GetItemsAsync<T, TWithId>(query, cancellationToken)).FirstOrDefault();
        }
    }
}
