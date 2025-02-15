using Chapi.Api.Models;

namespace Chapi.Api.Services.CrudServices
{
    public interface IDatabaseItemService<T> where T : IDatabaseItemWithId
    {
        Task<T> CreateItem(T item, CancellationToken cancellationToken = default);
        Task DeleteItem(T item, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllItems(CancellationToken cancellationToken = default);
        Task<T?> GetItemIfExists(T item, CancellationToken cancellationToken = default);
        Task<T> GetItem(T item, CancellationToken cancellationToken = default);
        Task<List<T>> GetItemsWhereKeyIsValue(string key, string value, CancellationToken cancellationToken = default);
        Task<T> PatchItem(T item, CancellationToken cancellationToken = default);
        Task<T> PutItem(T item, CancellationToken cancellationToken = default);
        Task<T> UpdateItem(T item, bool hard = false, CancellationToken cancellationToken = default);
    }
}