using System;
using System.Collections.Generic;

namespace ProjekatSP
{
    public class Kes
    {
        private static readonly object lockObject = new object();
        private static readonly Dictionary<string, IQAir> cache = new Dictionary<string, IQAir>();

        public static bool Contains(string key)
        {
            lock (lockObject)
            {
                return cache.ContainsKey(key);
            }
        }

        public static IQAir ReadFromCache(string key)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out IQAir value) && value != null)
                    return value;
                else
                    throw new KeyNotFoundException($"Key '{key}' not found in cache.");
            }
        }

        public static void WriteToCache(string key, IQAir value)
        {
            lock (lockObject)
            {
                cache[key] = value;
            }
        }
    }
}
