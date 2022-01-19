using RedisClient.Abstractions;
using RedisClient.Models.Consts;
using RedisClient.Models.Enums;
using RedisClient.Models.Exceptions;
using RedisClient.Models.RedisResults;
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
        public async Task<OperationResult<string>> SetAsync(string key, string value, TimeSpan? expiry = null, bool keepttl = false, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default)
        {
            ThrowIfKeyInvalid(key);
            if (returnOldValue && writeBehavior == KeyWriteBehavior.NotExists)
            {
                throw new RedisCommandSyntaxErrorException("syntax error");
            }

            var expiryArg = "";
            double expiryVal = -1;
            if (keepttl)
            {
                expiryArg = "KEEPTTL";
            }
            else if (expiry.HasValue)
            {
                expiryArg = "PX";
                expiryVal = expiry.Value.TotalMilliseconds;
            }
            var writeBehave = "";
            if (writeBehavior == KeyWriteBehavior.Exists)
            {
                writeBehave = "XX";
            }
            else if (writeBehavior == KeyWriteBehavior.NotExists)
            {
                writeBehave = "NX";
            }
            var get = returnOldValue ? "GET" : "";
            var parameters = new { key = (RedisKey)key, value, expiryArg, expiryVal, writeBehave, get };

            var luaScript = LuaScript.Prepare(SETLuaScript);
            var redisVal = await database.ScriptEvaluateAsync(luaScript, parameters);

            /*
             Simple string reply: OK if SET was executed correctly.
             Null reply: (nil) if the SET operation was not performed because the user specified the NX or XX option but the condition was not met.
             If the command is issued with the GET option, the above does not apply. It will instead reply as follows, regardless if the SET was actually performed:
             Bulk string reply: the old string value stored at key.
             Null reply: (nil) if the key did not exist.
             */
            if (returnOldValue)
            {
                if (redisVal.Type == ResultType.BulkString)
                {
                    if (redisVal.IsNull && writeBehavior == KeyWriteBehavior.Exists)
                    {
                        return new OperationResult<string>(false, "");
                    }
                    return new OperationResult<string>(true, redisVal.IsNull ? "" : redisVal.ToString()!);
                }
            }
            else
            {
                if (redisVal.Type == ResultType.SimpleString && redisVal.IsNull == false
                    && string.Equals(redisVal.ToString(), RedisReturnValue.OK, StringComparison.OrdinalIgnoreCase))
                {
                    return new OperationResult<string>(true, "");
                }
                else if (redisVal.Type == ResultType.BulkString && redisVal.IsNull)
                {
                    return new OperationResult<string>(false, "");
                }
            }

            throw new RedisUnsupportedReturnValueException($"GET return type is {redisVal.Type}, value is {redisVal.IsNull}");
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
            ThrowIfKeyInvalid(key);
            var luaScript = LuaScript.Prepare(GETEXLuaScript);

            var expiryArg = "PERSIST";
            double expiryVal = -1;
            if (expiry.HasValue)
            {
                expiryArg = "PX";
                expiryVal = expiry.Value.TotalMilliseconds;
            }

            var redisVal = await database.ScriptEvaluateAsync(luaScript, new { key = (RedisKey)key, expiryArg, expiryVal });
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

        #region lua scripts
        private const string SETLuaScript = @"local command = 'SET'
if @get == 'GET' then
    if @expiryArg == 'KEEPTTL' then
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @expiryArg, @writeBehave, @get)
        else
            return redis.pcall(command, @key, @value, @expiryArg, @get)
        end
    elseif @expiryArg ~= '' then
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @expiryArg, @expiryVal,
                               @writeBehave, @get)
        else
            return redis.pcall(command, @key, @value, @expiryArg, @expiryVal, @get)
        end
    else
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @writeBehave, @get)
        else
            return redis.pcall(command, @key, @value, @get)
        end
    end
else
    if @expiryArg == 'KEEPTTL' then
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @expiryArg, @writeBehave)
        else
            return redis.pcall(command, @key, @value, @expiryArg)
        end
    elseif @expiryArg ~= '' then
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @expiryArg, @expiryVal,
                               @writeBehave)
        else
            return redis.pcall(command, @key, @value, @expiryArg, @expiryVal)
        end
    else
        if @writeBehave ~= '' then
            return redis.pcall(command, @key, @value, @writeBehave)
        else
            return redis.pcall(command, @key, @value)
        end
    end
end";

        private const string GETEXLuaScript = @"if (@expiryArg == 'PERSIST') then
    return redis.pcall('GETEX', @key, @expiryArg)
else
    return redis.pcall('GETEX', @key, @expiryArg, @expiryVal)
end";
        #endregion
    }
}
