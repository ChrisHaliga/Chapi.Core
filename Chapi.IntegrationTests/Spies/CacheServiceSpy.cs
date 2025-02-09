namespace Chapi.IntegrationTests.Spies
{
    using Chapi.Api.Services;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class CacheServiceSpy : ICacheService, IServiceSpy
    {
        private readonly CacheService _innerCacheService;

        private readonly Dictionary<string, object?> _registry = new();
        public List<HistoryRecord> History { get; } = new(); // Logs method calls

        public CacheServiceSpy(IDistributedCache cacheService)
        {
            _innerCacheService = new CacheService(cacheService);
        }

        public object? GetInstanceOfHistoryItem(string key)
        {
            if (_registry.ContainsKey(key))
            {
                return _registry[key];
            }
            return default;
        }

        private void UpdateHistory(string method, string key, bool isSuccess, object? item)
        {
            if (!_registry.ContainsKey(key))
            {
                _registry.Add(key, item);
            }

            History.Add(new HistoryRecord(method, key, isSuccess));
        }

        public async Task Create(string key, object value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        {
            await _innerCacheService.Create(key, value, options, cancellationToken);
            UpdateHistory(nameof(Create), key, true, value);
        }

        public async Task<T?> Get<T>(string key, CancellationToken cancellationToken = default)
        {
            var value = await _innerCacheService.Get<T>(key, cancellationToken);
            UpdateHistory(nameof(Get), key, true, value);
            return value;
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            UpdateHistory(nameof(Remove), key, true, null);
            await _innerCacheService.Remove(key, cancellationToken);
        }
    }
}
