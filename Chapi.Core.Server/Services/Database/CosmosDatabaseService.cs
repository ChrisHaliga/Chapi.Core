﻿using Chapi.Api.Models;
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

        public async Task<T?> GetItemAsync<T>(T item, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            return await _cosmosWrapper.GetItemAsync(item, cancellationToken);
        }

        public async Task<List<T>> ListItemsAsync<T>(string key, string value, CancellationToken cancellationToken = default)  where T : IDatabaseItemWithId
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                throw new BadRequestException("Must provide key value pair");
            }

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{key} = @providedkey").WithParameter("@providedkey", value);

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
