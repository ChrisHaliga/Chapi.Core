using Chapi.Api.Models;

namespace Chapi.Api.Services.Database
{
    public interface IDatabaseService
    {
        Task<T> CreateItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task<T?> GetItemAsync<T, TWithId>(KeyValuePair<string, string> query, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task<T?> GetItemByIdAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task<List<TWithId>> GetItemsAsync<T, TWithId>(KeyValuePair<string, string> query, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task<List<TWithId>> GetAllItemsAsync<T, TWithId>(CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task<T> UpdateItemAsync<T, TWithId>(T item, bool hard = false, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
        Task DeleteItemAsync<T, TWithId>(T item, CancellationToken cancellationToken = default) where T : DatabaseItem<TWithId> where TWithId : DatabaseItemWithId;
    }
}
