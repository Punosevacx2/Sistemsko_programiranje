using System;
using System.Collections.Generic;

namespace ProjekatSP
{
    public class Kes
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(1);
        private static readonly object lockObject = new object();
        private static readonly Dictionary<string, (IQAir value, DateTime cachedAt)> cache =
            new Dictionary<string, (IQAir, DateTime)>();

        public static bool Contains(string key)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out var entry))
                    return DateTime.UtcNow - entry.cachedAt < Ttl;
                return false;
            }
        }

        public static IQAir ReadFromCache(string key)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out var entry) && DateTime.UtcNow - entry.cachedAt < Ttl)
                    return entry.value;
                throw new KeyNotFoundException($"Key '{key}' not found in cache or expired.");
            }
        }

        public static void WriteToCache(string key, IQAir value)
        {
            lock (lockObject)
            {
                cache[key] = (value, DateTime.UtcNow);
            }
        }
    }
}
