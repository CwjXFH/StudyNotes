using RedisClient.Abstractions;
using StackExchange.Redis;

namespace RedisClient.StackExchange.Internal
{
    internal class RedisKeyOperator : RedisOperator, IRedisKeyOperator
    {
        public RedisKeyOperator(IDatabase database)
            : base(database) { }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.KeyExistsAsync(key);
        }

        public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.KeyDeleteAsync(key);
        }

    }
}
