using Chapi.Api.Models;

namespace Chapi.Api.Services.ApiServices
{
    public interface IApiService<T> where T : IDatabaseItemWithId
    {
        Task<T> GetItem(T item, CancellationToken cancellationToken = default);
        Task<List<T>> GetItemByPartition(string parent, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllItems(CancellationToken cancellationToken = default);
        Task<T> CreateItem(T item, CancellationToken cancellationToken = default);
        Task<T> PatchItem(T item, CancellationToken cancellationToken = default);
        Task<T> PutItem(T item, CancellationToken cancellationToken = default);
        Task DeleteItem(T item, CancellationToken cancellationToken = default);

        Task<T> MigrateItem(T item, string newPartition, CancellationToken cancellationToken = default);

        Task<RelatedData> ValidateItem(T item, CancellationToken cancellationToken = default);
        Task UpdateRelatedItems(RelatedData? validatedData, T? before, T? after, CancellationToken cancellationToken = default);
    }
}
