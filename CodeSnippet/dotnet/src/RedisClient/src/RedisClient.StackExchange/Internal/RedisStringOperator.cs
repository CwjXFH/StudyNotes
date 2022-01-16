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
            ThrowIfKeyInvalid(key);
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

        public async Task MSetAsync(IDictionary<string, string> keyValues, CancellationToken cancellationToken = default)
            => await MSetCoreAsync(keyValues, When.Always, cancellationToken);

        public async Task<bool> MSetNXAsync(IDictionary<string, string> keyValues, CancellationToken cancellationToken = default)
            => await MSetCoreAsync(keyValues, When.NotExists, cancellationToken);

        private async Task<bool> MSetCoreAsync(IDictionary<string, string> keyValues, When when, CancellationToken cancellationToken = default)
        {
            if (keyValues == null || keyValues.Count <= 0)
            {
                throw new ArgumentException("param must have values", $"{nameof(keyValues)}");
            }
            var kvPairs = new List<KeyValuePair<RedisKey, RedisValue>>(keyValues.Count);
            foreach (var kv in keyValues)
            {
                ThrowIfKeyInvalid(kv.Key);
                kvPairs.Add(new KeyValuePair<RedisKey, RedisValue>(kv.Key, kv.Value));
            }

            return await database.StringSetAsync(kvPairs.ToArray(), when);
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
            ThrowIfKeyInvalid(key);
            var redisVal = await GetAsync(key, cancellationToken);
            return JsonSerializer.Deserialize<T>(redisVal) ?? defaultValue;
        }

        public async Task<string> GetRangeAsync(string key, int start, int end, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetRangeAsync(key, start, end);
        }

        public async Task<OperationResult<IDictionary<string, string>>> MGetAsync(ICollection<string> keys, CancellationToken cancellationToken = default)
        {
            if (keys == null || keys.Count <= 0)
            {
                throw new ArgumentException("param must have values", $"{nameof(keys)}");
            }
            var redisKeys = new List<RedisKey>(keys.Count);
            foreach (var key in keys)
            {
                ThrowIfKeyInvalid(key);
                redisKeys.Add(new RedisKey(key));
            }
            var redisVal = await database.StringGetAsync(redisKeys.ToArray());
            var result = new OperationResult<IDictionary<string, string>>(true, new Dictionary<string, string>());
            if (redisVal != null && redisVal.Length > 0)
            {
                for (var i = 0; i < redisKeys.Count; i++)
                {
                    var key = redisKeys[i];
                    var val = redisVal[i];
                    if (val.HasValue)
                    {
                        result.Data[key] = val.ToString();
                    }
                    else
                    {
                        result.Data[key] = "";
                    }
                }
            }
            return result;
        }

        public async Task<long> StrLenAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringLengthAsync(key);
        }
        #endregion

        #region Write & Read Operation
        public async Task<OperationResult<string>> SetAsync(string key, string value, TimeSpan? expiry, bool keepttl = false, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default)
        {
            if (returnOldValue)
            {
                throw new NotSupportedException();
            }
            if (keepttl)
            {
                throw new NotSupportedException();
            }
            ThrowIfKeyInvalid(key);
            var when = KeyWriteBehaviorConvert.ToWhen(writeBehavior);
            var writeResult = await database.StringSetAsync(key, value, expiry, when);
            return new OperationResult<string>(writeResult, "");
        }

        public async Task<OperationResult<T>> SetAsync<T>(string key, T value, TimeSpan? expiry, bool keepttl = false, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default) where T : class
        {
            var redisVal = JsonSerializer.Serialize(value);
            var writeResult = await SetAsync(key, redisVal, expiry, keepttl, writeBehavior, returnOldValue, cancellationToken);
            return new OperationResult<T>(writeResult.Successed, default!);
        }

        public async Task<string> GetDelAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            return await database.StringGetDeleteAsync(key);
        }

        public async Task<string> GetEXAsync(string key, TimeSpan? expiry, CancellationToken cancellationToken = default)
        {
            using var fs = File.Open("./Lua/String/GETEX.lua", FileMode.Open);
            using var sr = new StreamReader(fs);
            var lua = sr.ReadToEnd();

            var expiryArg = "PERSIST";
            double expiryVal = -1;
            if (expiry.HasValue)
            {
                expiryArg = "PX";
                expiryVal = expiry.Value.TotalMilliseconds;
            }

            var redisVal = await database.ScriptEvaluateAsync(lua, new RedisKey[] { key }, new RedisValue[] { expiryArg, expiryVal });
            if (redisVal == null || redisVal.IsNull)
            {
                return "";
            }
            else
            {
                return redisVal.ToString()!;
            }
        }
        #endregion

    }
}
