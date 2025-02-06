using Azure.Identity;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Wrappers;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services
{
    public interface IDatabaseService
    {
        Task CreateItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto;
        Task<Dto?> GetItemAsync<T, Dto>(T item, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto;
        Task UpdateItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto;
        Task DeleteItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto;
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

        public async Task CreateItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            await CosmosService(databaseName, containerName).CreateItemAsync<T, Dto>(item, cancellationToken);
        }

        public async Task<Dto?> GetItemAsync<T, Dto>(T item, string databaseName, string containerName, QueryDefinition? query = null, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            return await CosmosService(databaseName, containerName).GetItemAsync<T, Dto>(item, query, cancellationToken);
        }


        public async Task UpdateItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            await CosmosService(databaseName, containerName).UpdateItemAsync<T, Dto>(item, cancellationToken);
        }

        public async Task DeleteItemAsync<T, Dto>(T item, string databaseName, string containerName, CancellationToken cancellationToken = default) where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
        {
            await CosmosService(databaseName, containerName).DeleteItemAsync<T, Dto>(item, cancellationToken);
        }
    }
}
