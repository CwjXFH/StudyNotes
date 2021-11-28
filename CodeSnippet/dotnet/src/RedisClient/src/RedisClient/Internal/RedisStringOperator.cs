using RedisClient.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisClient.Internal
{
    internal class RedisStringOperator : RedisOperator, IRedisStringOperator
    {
        public RedisStringOperator(IDatabase database)
            : base(database) { }

        public async Task<bool> SetAsync(string key, string value)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringSetAsync(key, value);
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan expiry)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringSetAsync(key, value, expiry);
        }

        public async Task<string> GetAsync(string key)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetAsync(key);
        }

        public async Task<T> GetAsync<T>(string key, T defaultValue)
        {
            ThrowIfKeyInvalid(key);
            var value = await database.StringGetAsync(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }
            return JsonSerializer.Deserialize<T>(value) ?? defaultValue;
        }


    }
}
