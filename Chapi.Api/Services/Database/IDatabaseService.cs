using Chapi.Api.Models;

namespace Chapi.Api.Services.Database
{
    public interface IDatabaseService
    {
        Task<T> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task<T?> GetItemByQueryAsync<T>(KeyValuePair<string, string> query, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task<T?> GetItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task<List<T>> ListItemsAsync<T>(string key, string value, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task<List<T>> ListAllItemsAsync<T>(CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task<T> UpdateItemAsync<T>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
        Task DeleteItemAsync<T>(T item, CancellationToken cancellationToken = default) where T : IDatabaseItemWithId;
    }
}

