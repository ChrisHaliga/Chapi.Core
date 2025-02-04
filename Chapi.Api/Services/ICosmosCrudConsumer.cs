using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services
{
    public interface ICosmosCrudConsumer<T>
    {        
        public Task CreateAsync(T item, CancellationToken cancellationToken = default);
        public Task<T?> GetAsync(string id, CancellationToken cancellationToken = default);
        public Task UpdateAsync(T item, CancellationToken cancellationToken = default);
        public Task DeleteAsync(T item, CancellationToken cancellationToken = default);
    }
}
