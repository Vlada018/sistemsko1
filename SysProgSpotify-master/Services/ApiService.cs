using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SysProg.Services
{
    public class ApiService
    {
        private readonly string baseUrl;
        private readonly string apiKey;
        private readonly HttpClient client;
        private static ConcurrentDictionary<string, List<JObject>> cache = new ConcurrentDictionary<string, List<JObject>>();
        private static ConcurrentDictionary<string, Timer> cacheTimers = new ConcurrentDictionary<string, Timer>();

        public ApiService(string baseUrl, string apiKey)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public List<JObject> FetchDataForQueries(List<string> queries, List<string> types)
        {
            string cacheKey = $"{string.Join(",", queries)}-{string.Join(",", types)}";
            if (cache.TryGetValue(cacheKey, out var cachedResults))
            {
                Console.WriteLine("Cache hit.");
                return cachedResults;
            }

            Console.WriteLine("Cache miss.");
            var joinedTypes = string.Join(",", types);

            var results = new List<JObject>();
            var countDownEvent = new CountdownEvent(queries.Count);

            foreach (var query in queries)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        string url = $"{baseUrl}?q={Uri.EscapeDataString(query)}&type={joinedTypes}";
                        var response = client.GetAsync(url).Result;
                        var resBody = response.Content.ReadAsStringAsync().Result;
                        var result = JObject.Parse(resBody);
                        lock (results)
                        {
                            results.Add(result);
                        }
                    }
                    finally
                    {
                        countDownEvent.Signal();
                    }
                });
            }

            countDownEvent.Wait();
            cache[cacheKey] = results;
            SetCacheExpiration(cacheKey);

            return results;
        }

        private void SetCacheExpiration(string cacheKey)
        {
            Timer timer = new Timer(RemoveFromCache, cacheKey, TimeSpan.FromMinutes(10), Timeout.InfiniteTimeSpan);
            cacheTimers[cacheKey] = timer;
        }

        private void RemoveFromCache(object state)
        {
            string cacheKey = (string)state;
            cache.TryRemove(cacheKey, out _);
            cacheTimers.TryRemove(cacheKey, out _);
            Console.WriteLine($"Cache entry for key '{cacheKey}' removed.");
        }
    }
}
