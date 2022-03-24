using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model.Cache
{
    public abstract class FlushableCache<K, V>
        where K : notnull
        where V : IKeyedItem<K>
    {
        protected ConcurrentDictionary<K, V> _cache;

        public int Count => _cache.Count;

        public FlushableCache()
        {
            _cache = new();
        }

        public async Task<V?> GetAsync(K key)
        {
            V? result;
            var success = _cache.TryGetValue(key, out result);

            if (success)
            {
                return await Task.FromResult(result);
            }

            return await Task.FromResult(default(V));
        }

        public async Task<V?> GetOrCreateAsync(K key)
        {
            var result = await GetAsync(key);

            if (!EqualityComparer<V>.Default.Equals(result, default(V)))
            {
                return result;
            }

            result = await CreateAsync(key);

            if (result != null)
            {
                Put(result);
            }

            return result;
        }

        public abstract Task<V?> CreateAsync(K key);

        public void Put(V item)
        {
            Flush(item);
            _cache.TryAdd(item.Key, item);
        }

        public void Flush(V item)
        {
            Flush(item.Key);
        }

        public void Flush(K key)
        {
            _cache.TryRemove(key, out _);
        }
    }
}