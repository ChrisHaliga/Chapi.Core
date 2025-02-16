using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chapi.Api.Services
{
    public interface ICacheService
    {
        Task Create(string key, object value, DistributedCacheEntryOptions? distributedCacheEntryOptions = null, CancellationToken cancellationToken = default);
        Task<T?> Get<T>(string key, CancellationToken cancellationToken = default);
        Task Remove(string key, CancellationToken cancellationToken = default);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        private static DistributedCacheEntryOptions DefaultDistributedCacheEntryOptions => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
            SlidingExpiration = TimeSpan.FromHours(1)
        };

        public async Task Create(string key, object value, DistributedCacheEntryOptions? distributedCacheEntryOptions = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var ms = new MemoryStream();
            JsonSerializer.Serialize(ms, value, jsonSerializerOptions);
            ms.Position = 0;

            await _cache.SetAsync(key, ms.ToArray(), distributedCacheEntryOptions ?? DefaultDistributedCacheEntryOptions, cancellationToken);
        }

        public async Task<T?> Get<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = await _cache.GetAsync(key, cancellationToken);
            if (data == null)
            {
                return default;
            }

            using var ms = new MemoryStream(data);
            var deserialized = JsonSerializer.Deserialize<T>(ms, jsonSerializerOptions);

            if (deserialized is T result)
            {
                return result;
            }

            return default;
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _cache.RemoveAsync(key, cancellationToken);
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Omits null values
        };
    }
}
