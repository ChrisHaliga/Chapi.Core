using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public abstract class CrudController<T, TWithId> : ControllerBase where T : CosmosItem<TWithId> where TWithId : CosmosItemWithId
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _databaseName;
        private readonly string _containerName;

        public CrudController(IDatabaseService cosmosService, CrudConfigData<T> config)
        {
            _databaseService = cosmosService;
            _databaseName = config.DatabaseName;
            _containerName = config.ContainerName;
        }

        internal async Task<T?> GetItem(T item, CancellationToken cancellationToken = default)
        {
            return await _databaseService.GetItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken: cancellationToken);
        }

        internal async Task CreateItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.CreateItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }

        internal async Task UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default)
        {
            await _databaseService.UpdateItemAsync<T, TWithId>(item, _databaseName, _containerName, hard, cancellationToken);
        }

        internal async Task DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            await _databaseService.DeleteItemAsync<T, TWithId>(item, _databaseName, _containerName, cancellationToken);
        }
    }
}
