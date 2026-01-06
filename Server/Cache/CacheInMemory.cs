using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Server.Cache
{
    public class CacheInMemory : ICache
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public void Add(string key, object value)
        {
            _cache[key] = value;
        }

        public ICollection<string> GetAllKeys()
        {
            return _cache.Keys;
        }

        public object Get(string key)
        {
            _cache.TryGetValue(key, out var value);

            return value;
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }
    }
}