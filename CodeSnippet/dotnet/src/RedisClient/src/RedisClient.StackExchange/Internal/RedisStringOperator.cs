using RedisClient.Abstractions;
using RedisClient.Models;
using RedisClient.Models.Enums;
using RedisClient.StackExchange.Convertor;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisClient.StackExchange.Internal
{
    internal class RedisStringOperator : RedisOperator, IRedisStringOperator
    {
        /// <summary>
        /// 2^29 - 1
        /// </summary>
        private const uint MaxSetRagneOffset = (2 << 28) - 1;

        public RedisStringOperator(IDatabase database)
            : base(database) { }

        #region Write Operation
        public async Task<long> AppendAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            return await database.StringAppendAsync(key, value);
        }

        public async Task<long> IncrByAsync(string key, long increment = 1, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringIncrementAsync(key, increment);
        }

        public async Task<long> DecrByAsync(string key, long increment = 1, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringDecrementAsync(key, increment);
        }

        public async Task<double> IncrByFloatAsync(string key, double increment, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringIncrementAsync(key, increment);
        }

        public async Task<long> SetRangeAsync(string key, uint offset, string value, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            if (offset > MaxSetRagneOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"The maximum allowed offset is {MaxSetRagneOffset}");
            }
            var result = await database.StringSetRangeAsync(key, offset, value);
            return (long)result;
        }

        public async Task<string> GetRangeAsync(string key, int start, int end, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetRangeAsync(key, start, end);
        }
        #endregion

        #region Read Operation
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

        public async Task<long> StrLenAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringLengthAsync(key);
        }
        #endregion

        #region Write & Read Operation
        public async Task<OperationResult<string>> SetAsync(string key, string value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default)
        {
            if (returnOldValue)
            {
                throw new NotSupportedException();
            }
            ThrowIfKeyInvalid(key);
            var when = KeyWriteBehaviorConvert.ToWhen(writeBehavior);
            var writeResult = await database.StringSetAsync(key, value, expiry, when);
            return new OperationResult<string>(writeResult, "");
        }

        public async Task<OperationResult<T>> SetAsync<T>(string key, T value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default) where T : class
        {
            var redisVal = JsonSerializer.Serialize(value);
            var writeResult = await SetAsync(key, redisVal, expiry, writeBehavior, returnOldValue, cancellationToken);
            return new OperationResult<T>(writeResult.Successed, default);
        }

        public async Task<string> GetDelAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetDeleteAsync(key);
        }

        public Task<string> GetEXAsync(string key, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
        #endregion

    }
}
