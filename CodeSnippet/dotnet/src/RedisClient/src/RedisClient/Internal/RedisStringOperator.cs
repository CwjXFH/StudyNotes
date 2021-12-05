using RedisClient.Abstractions;
using RedisClient.Convertor;
using RedisClient.Models.Enums;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisClient.Internal
{
    internal class RedisStringOperator : RedisOperator, IRedisStringOperator
    {
        public RedisStringOperator(IDatabase database)
            : base(database) { }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            var when = KeyWriteBehaviorConvert.ToWhen(writeBehavior);
            return await database.StringSetAsync(key, value, expiry, when);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None, CancellationToken cancellationToken = default)
        {
            var redisVal = JsonSerializer.Serialize(value);
            return await SetAsync(key, redisVal, expiry, writeBehavior, cancellationToken);
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetAsync(key);
        }

        public async Task<T> GetAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default)
        {
            var redisVal = await GetAsync(key, cancellationToken);
            return JsonSerializer.Deserialize<T>(redisVal) ?? defaultValue;
        }
    }
}
