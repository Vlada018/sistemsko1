using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
            var client = new HttpClient();
            var joinedTypes = string.Join(",", types);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var tasks = new List<Task<JObject>>();
            foreach (var query in queries)
            {
                string url = $"{baseUrl}?q={Uri.EscapeDataString(query)}&type={joinedTypes}";
                var task = client.GetAsync(url).ContinueWith(responseTask =>
                {
                    var resBody = responseTask.Result.Content.ReadAsStringAsync().Result;
                    return JObject.Parse(resBody);
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            var results = tasks.Select(t => t.Result).ToList();

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
