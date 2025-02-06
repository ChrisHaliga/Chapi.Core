using Chapi.Api.Models;

namespace Chapi.Api.Services
{
    public interface ICosmosCrudConsumer<T, Dto> where T : DatabaseCompliantObject<Dto> where Dto : DatabaseDto
    {        
        public Task CreateAsync(T item, CancellationToken cancellationToken = default);
        public Task<Dto?> GetAsync(string id, CancellationToken cancellationToken = default);
        public Task UpdateAsync(T item, CancellationToken cancellationToken = default);
        public Task DeleteAsync(T item, CancellationToken cancellationToken = default);
    }
}
