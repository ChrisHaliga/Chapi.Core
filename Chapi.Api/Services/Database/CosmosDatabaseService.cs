using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Wrappers;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services.Database
{
    internal class CosmosDatabaseService : IDatabaseService
    {
        private readonly RuntimeInfo _runtimeInfo;
        private readonly CosmosWrapper _cosmosWrapper;

        public static readonly string IdKey = "id";

        public CosmosDatabaseService(CosmosConfigData config, ICacheService cache, RuntimeInfo runtimeInfo, string databaseName, string containerName)
        {
            _cosmosWrapper = new CosmosWrapper(databaseName, containerName, config.CosmosDbUri, cache, runtimeInfo);
            _runtimeInfo = runtimeInfo;
        }

        public async Task<T> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            return await _cosmosWrapper.CreateItemAsync(item, cancellationToken);
        }

        public async Task<T?> GetItemByIdAsync<T>(string id, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId
        {
            return await GetItemByQueryAsync<T>(new KeyValuePair<string, string>(IdKey, id), cancellationToken);
        }

        public async Task<T?> GetItemByQueryAsync<T>(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            if (string.IsNullOrEmpty(keyValuePair.Key) || string.IsNullOrEmpty(keyValuePair.Value))
            {
                throw new BadRequestException("Must provide key value pair");
            }

            QueryDefinition query = new QueryDefinition($"SELECT * FROM c WHERE c.{keyValuePair.Key} = @providedkey").WithParameter("@providedkey", keyValuePair.Value);

            return await _cosmosWrapper.GetItemByQueryAsync<T>(query, cancellationToken);
        }

        public async Task<T?> GetItemAsync<T>(T item, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            return await _cosmosWrapper.GetItemAsync(item, cancellationToken);
        }

        public async Task<List<T>> ListItemsAsync<T>(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            if (string.IsNullOrEmpty(keyValuePair.Key) || string.IsNullOrEmpty(keyValuePair.Value))
            {
                throw new BadRequestException("Must provide key value pair");
            }

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{keyValuePair.Key} = @providedkey").WithParameter("@providedkey", keyValuePair.Value);

            return await _cosmosWrapper.ListItemsAsync<T>(query, cancellationToken);
        }

        public async Task<List<T>> ListAllItemsAsync<T>(CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            return await _cosmosWrapper.ListItemsAsync<T>(new QueryDefinition($"SELECT * FROM c"), cancellationToken);
        }

        public async Task<T> UpdateItemAsync<T>(T item, bool hard = false, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            return await _cosmosWrapper.UpdateItemAsync<T>(item, hard, cancellationToken);
        }

        public async Task DeleteItemAsync<T>(T item, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            await _cosmosWrapper.DeleteItemAsync<T>(item, cancellationToken);
        }
    }
}
