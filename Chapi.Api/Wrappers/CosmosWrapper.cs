using Azure.Identity;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services;
using Chapi.Api.Services.Database;
using Microsoft.Azure.Cosmos;
using System.Threading;

namespace Chapi.Api.Wrappers
{
    public class CosmosWrapper
    {
        private readonly string _databaseName;
        private readonly string _containerName;
        private readonly ICacheService _cache;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly RuntimeInfo _runtimeInfo;

        public CosmosWrapper(string databaseName, string containerName, string CosmosDbUri, ICacheService cache, RuntimeInfo runtimeInfo)
        {
            _databaseName = databaseName;
            _containerName = containerName;
            _cosmosClient = new CosmosClient(CosmosDbUri, new DefaultAzureCredential(), new CosmosClientOptions() { SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } });
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            _cache = cache;
            _runtimeInfo = runtimeInfo;
        }

        public async Task<T> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _container.CreateItemAsync<T>(item, new PartitionKey(item.GetPartitionKey()), cancellationToken: cancellationToken);
            }
            catch(CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    if(_runtimeInfo.IsDevelopment)
                    {
                        throw new ConflictException(item, e);
                    }
                    throw new ConflictException(item);
                }

                throw;
            }

            await _cache.Create(GetCacheKey(item.Id, _databaseName, _containerName), item, cancellationToken: cancellationToken);

            return item;
        }

        public async Task<T?> GetItemByIdAndPartitionKeyAsync<T>(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = GetCacheKey(id, _databaseName, _containerName);
            var cached = await _cache.Get<T>(cacheKey, cancellationToken: cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                var foundItem = await _container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);
                if (foundItem != null)
                {
                    await _cache.Create(cacheKey, foundItem, cancellationToken: cancellationToken);
                    return foundItem;
                }
            }
            catch(CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (_runtimeInfo.IsDevelopment)
                    {
                        throw new NotFoundException(id, e);
                    }
                    throw new NotFoundException(id);
                }

                throw;
            }

            return default;
        }

        public async Task<T?> GetItemByQueryAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var foundItem = (await ListItemsAsync<T>(query, cancellationToken)).FirstOrDefault();

            if (foundItem == null)
            {
                throw new NotFoundException();

            }

            if (!string.IsNullOrEmpty(foundItem.Id))
            {
                await _cache.Create(GetCacheKey(foundItem.Id, _databaseName, _containerName), foundItem);
            }

            return foundItem;
        }

        public async Task<T?> GetItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            var id = item.GetId();
            var partitionKey = item.GetPartitionKey();

            if(string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(item, "DatabaseItems must have an Id to be queried individually");
            }

            if (!string.IsNullOrEmpty(id))
            {
                return await GetItemByIdAndPartitionKeyAsync<T>(id, new PartitionKey(partitionKey), cancellationToken);
            }

            return await GetItemByQueryAsync<T>(new QueryDefinition($"SELECT * FROM c WHERE c.{CosmosDatabaseService.IdKey} = @Id").WithParameter("@Id", item.Id), cancellationToken);
        }

        public async Task<List<T>> ListItemsAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var feedIterator = _container.GetItemQueryIterator<T>(query);

                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync(cancellationToken);

                    if (response.Resource.Any())
                    {
                        return response.Resource.ToList();
                    }
                }
            }
            catch(CosmosException e)
            {
                if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw; //Not found in list, should just be an empty list, otherwise throw.
                }
            }

            return new List<T>();
        }

        public async Task<T> UpdateItemAsync<T>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = await GetItemAsync(item, cancellationToken: cancellationToken);

            if (existing == null)
            {
                await CreateItemAsync(item, cancellationToken);
                return item;
            }

            InjectValues(item, existing, hard);

            try
            {
                await _container.UpsertItemAsync(existing, new PartitionKey(existing.GetPartitionKey()), cancellationToken: cancellationToken);
            }
            catch(CosmosException e)
            {
                if(e.StatusCode == System.Net.HttpStatusCode.BadRequest || e.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    if (_runtimeInfo.IsDevelopment)
                    {
                        throw new BadRequestException(item, "Request was bad or there was a conflict", e);
                    }
                    throw new BadRequestException(item, "Request was bad or there was a conflict");
                }

                throw;
            }
            await _cache.Create(GetCacheKey(item.Id, _databaseName, _containerName), existing, cancellationToken: cancellationToken);
            return existing;
        }

        public async Task DeleteItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(item.GetId()) || string.IsNullOrEmpty(item.GetPartitionKey()))
            {
                throw new BadRequestException(item, "DatabaseItems must have an Id and Partition Key");
            }

            try
            {
                await _container.DeleteItemAsync<T>(item.GetId(), new PartitionKey(item.GetPartitionKey()), cancellationToken: cancellationToken);
            }
            catch(CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (_runtimeInfo.IsDevelopment)
                    {
                        throw new NotFoundException(item, e);
                    }
                    throw new NotFoundException(item);
                }

                throw;
            }

            await _cache.Remove(GetCacheKey(item.Id, _databaseName, _containerName));
        }

        private static void InjectValues<T1, T2>(T1 from, T2 to, bool hard = false)
        {
            if (from == null || to == null) return;

            var properties = from.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(from);
                if (hard || value != null)
                {
                    to.GetType().GetProperty(property.Name)?.SetValue(to, value);
                }
            }
        }

        private static string GetCacheKey(string? id, string databaseName, string containerName) => $"{databaseName}-{containerName}-{id}";
    }
}
