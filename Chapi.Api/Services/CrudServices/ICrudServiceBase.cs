using Chapi.Api.Models;

namespace Chapi.Api.Services.CrudServices
{
    public interface ICrudServiceBase<T> where T : IDatabaseItemWithId
    {
        Task<T> CreateItem(T item, CancellationToken cancellationToken = default);
        Task DeleteItem(T item, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllItems(CancellationToken cancellationToken = default);
        Task<T?> GetItemIfExists(T item, CancellationToken cancellationToken = default);
        Task<T> GetItem(T item, CancellationToken cancellationToken = default);
        Task<T> GetItemById(string id, CancellationToken cancellationToken = default);
        Task<List<T>> GetItemsByPartitionKey(string partitionKey, CancellationToken cancellationToken = default);
        Task<List<T>> GetItemsWhereKeyIsValue(KeyValuePair<string, string> keyValuePair, CancellationToken cancellationToken = default);
        Task<T> PatchItem(T item, CancellationToken cancellationToken = default);
        Task<T> PutItem(T item, CancellationToken cancellationToken = default);
        Task<T> UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default);
    }
}