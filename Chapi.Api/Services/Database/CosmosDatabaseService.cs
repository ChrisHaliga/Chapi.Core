using Azure.Identity;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Wrappers;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Chapi.Api.Services.Database
{
    internal class CosmosDatabaseService : IDatabaseService
    {
        private readonly CacheService _cache;
        private readonly RuntimeInfo _runtimeInfo;
        private readonly CosmosWrapper _cosmosWrapper;

        public CosmosDatabaseService(CosmosConfigData config, CacheService cache, RuntimeInfo runtimeInfo, string databaseName, string containerName)
        {
            _cosmosWrapper = new CosmosWrapper(databaseName, containerName, config.CosmosDbUri, cache, runtimeInfo);
            _cache = cache;
            _runtimeInfo = runtimeInfo;
        }

        public async Task<T> CreateItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            return await _cosmosWrapper.CreateItemAsync<T, TWithId>(item, cancellationToken);
        }

        public async Task<T?> GetItemAsync<T, TWithId>(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            if (string.IsNullOrEmpty(keyValuePair.Key) || string.IsNullOrEmpty(keyValuePair.Value))
            {
                throw new BadRequestException("Must provide key value pair");
            }

            QueryDefinition query = new QueryDefinition($"SELECT * FROM c WHERE c.{keyValuePair.Key} = @providedkey").WithParameter("@providedkey", keyValuePair.Value);

            return await _cosmosWrapper.GetItemAsync<T, TWithId>(query, cancellationToken);
        }

        public async Task<T?> GetItemByIdAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            return await _cosmosWrapper.GetItemByIdAsync<T, TWithId>(item, cancellationToken);
        }

        public async Task<List<TWithId>> GetItemsAsync<T, TWithId>(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            if (string.IsNullOrEmpty(keyValuePair.Key) || string.IsNullOrEmpty(keyValuePair.Value))
            {
                throw new BadRequestException("Must provide key value pair");
            }

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{keyValuePair.Key} = @providedkey").WithParameter("@providedkey", keyValuePair.Value);

            return await _cosmosWrapper.GetItemsAsync<T, TWithId>(query, cancellationToken);
        }

        public async Task<List<TWithId>> GetAllItemsAsync<T, TWithId>(CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            return await _cosmosWrapper.GetItemsAsync<T, TWithId>(new QueryDefinition($"SELECT * FROM c"), cancellationToken);
        }

        public async Task<T> UpdateItemAsync<T, TWithId>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            return await _cosmosWrapper.UpdateItemAsync<T, TWithId>(item, hard, cancellationToken);
        }

        public async Task DeleteItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId
        {
            await _cosmosWrapper.DeleteItemAsync<T, TWithId>(item, cancellationToken);
        }
    }
}
