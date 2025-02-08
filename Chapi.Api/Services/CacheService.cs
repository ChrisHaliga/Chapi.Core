using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Chapi.Api.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        private static DistributedCacheEntryOptions DefaultDistributedCacheEntryOptions => new DistributedCacheEntryOptions
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
            return JsonSerializer.Deserialize<T>(ms, jsonSerializerOptions);
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _cache.RemoveAsync(key, cancellationToken);
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Omits null values
        };
    }
}
