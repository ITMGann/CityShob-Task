using System.Collections.Generic;

namespace Server.Cache
{
    public interface ICache
    {
        void Add(string key, object value);
        ICollection<string> GetAllKeys();
        object Get(string key);
        void Remove(string key);
    }
}
