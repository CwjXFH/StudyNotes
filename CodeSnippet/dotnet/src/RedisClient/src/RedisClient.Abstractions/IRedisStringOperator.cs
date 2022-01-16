using RedisClient.Models;
using RedisClient.Models.Enums;

namespace RedisClient.Abstractions
{
    /// <summary>
    /// Operate redis string type
    /// </summary>
    /// <seealso href="https://redis.io/commands#string"/>
    public interface IRedisStringOperator
    {
        #region Write Operation
        /// <summary>
        /// If key already exists and is a string, this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string.
        /// </summary>
        /// <returns>The length of the string after the append operation.</returns>
        Task<long> AppendAsync(string key, string value, CancellationToken cancellationToken = default);
        /// <summary>
        /// Increments the number stored at key by increment, the default increment is one.
        /// If the key does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <returns>The value of key after the increment.</returns>
        Task<long> IncrByAsync(string key, long increment = 1, CancellationToken cancellationToken = default);
        /// <summary>
        /// Decrements the number stored at key by decrement, the default increment is one.
        /// If the key does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <returns>The value of key after the decrement.</returns>
        Task<long> DecrByAsync(string key, long increment = 1, CancellationToken cancellationToken = default);
        /// <summary>
        /// Increment the string representing a floating point number stored at key by the specified increment. 
        /// By using a negative increment value, the result is that the value stored at the key is decremented.
        /// If the key does not exist, it is set to 0 before performing the operation.
        /// </summary>
        /// <returns>The value of key after the increment.</returns>
        /// <remarks>
        /// In redis This operation output precision is fixed at 17 digits after the decimal point regardless of the actual
        /// internal precision of the computation. But double in C# has up to 15 decimal digits of precision, 
        /// although a maximum of 17 digits is maintained internally. 
        /// </remarks>
        Task<double> IncrByFloatAsync(string key, double increment, CancellationToken cancellationToken = default);
        /// <summary>
        /// Overwrites part of the string stored at key, starting at the specified offset, for the entire length of value. 
        /// If the offset is larger than the current length of the string at key, the string is padded with zero-bytes to make offset fit.
        /// </summary>
        /// <remarks>
        /// Non-existing keys are considered as empty strings, so this command will make sure it holds a string large enough to be able to set value at offset.
        /// </remarks>
        /// <returns>The length of the string after it was modified by the command.</returns>
        Task<long> SetRangeAsync(string key, uint offset, string value, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sets the given keys to their respective values. MSET replaces existing values with new values. 
        /// MSET is atomic, so all given keys are set at once.
        /// </summary>
        /// <returns>Always OK since MSET can't fail.</returns>
        Task MSetAsync(IDictionary<string, string> keyValues, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sets the given keys to their respective values. MSETNX will not perform any operation at all even if just a single key alread exists.
        /// MSETNX is atomic, so all given keys are set at once.
        /// </summary>
        /// <returns>True if the all keys were set. False if no key was set(at least one key already existed).</returns>
        Task<bool> MSetNXAsync(IDictionary<string, string> keyValues, CancellationToken cancellationToken = default);
        #endregion

        #region Read Operation
        /// <summary>
        /// Get the value of key. If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <returns>The value of key, or nil when key does not exist.</returns>
        Task<string> GetAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get the value of key. If the key does not exist the default value is returned.
        /// </summary>
        /// <typeparam name="T">The type of data stored in redis.</typeparam>
        /// <returns>The value of key, or default value.</returns>
        /// <exception cref="System.Text.Json.JsonException">
        /// The JSON is invalid.
        /// </exception>
        Task<T> GetAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns the substring of the string value stored at key, determined by the offsets start and end (both are inclusive). 
        /// Negative offsets can be used in order to provide an offset starting from the end of the string. 
        /// So -1 means the last character, -2 the penultimate and so forth.
        /// </summary>
        /// <remarks>
        /// The function handles out of range requests by limiting the resulting range to the actual length of the string.
        /// </remarks>
        Task<string> GetRangeAsync(string key, int start, int end, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns the values of all specified keys. 
        /// </summary>
        /// <remarks>
        /// For every key that does not hold a string value or does not exist, the special value nil is returned.
        /// Because of this, the operation never fails.
        /// </remarks>
        Task<OperationResult<IDictionary<string, string>>> MGetAsync(ICollection<string> keys, CancellationToken CancellationToken = default);
        /// <summary>
        /// Returns the length of the string value stored at key. An error is returned when key holds a non-string value.
        /// </summary>
        /// <returns>The length of the string at key, or 0 when key does not exist.</returns>
        Task<long> StrLenAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// The LCS command implements the longest common subsequence algorithm.
        /// </summary>
        /// <returns></returns>
        //Task<OperationResult<TResult>> LCSAsync<TResult>(string key1, string key2, bool onlyReturnMatchLen, bool returnIndex, long minMatchLen, bool withMatchLen, CancellationToken cancellationToken = default);
        #endregion

        #region Write & Read Operation
        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. 
        /// Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="keepttl">Retain the time to live associated with the key.</param>
        /// <param name="writeBehavior"><see cref="KeyWriteBehavior"/></param>
        /// <param name="returnOldValue">True return the old string stored at the key, or nil if key did not exist. Default is false.</param>
        /// <returns>True if value was set, otherwise false.</returns>
        Task<OperationResult<string>> SetAsync(string key, string value, TimeSpan? expiry, bool keepttl = false, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default);
        /// <summary>
        /// Set key to hold the value. If key already holds a value, it is overwritten, regardless of its type.
        /// Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="keepttl">Retain the time to live associated with the key.</param>
        /// <param name="writeBehavior"><see cref="KeyWriteBehavior"/></param>
        /// <param name="returnOldValue">True return the old string stored at the key, or nil if key did not exist. Default is false.</param>
        /// <returns>True if value was set, otherwise false.</returns>
        /// <exception cref="System.NotSupportedException">
        /// There is no compatible System.Text.Json.Serialization.JsonConverter for TValue  or its serializable members.
        /// </exception>
        Task<OperationResult<T>> SetAsync<T>(string key, T value, TimeSpan? expiry, bool keepttl = false, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None
            , bool returnOldValue = false, CancellationToken cancellationToken = default) where T : class;
        /// <summary>
        /// Get the value of key and delete the key.
        /// </summary>
        /// <returns>The value of key, nil when key does not exist, or an error if the key's value type isn't a string.</returns>
        Task<string> GetDelAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get the value of key and optionally set its expiration.
        /// </summary>
        /// <returns>The value of key, or nil when key does not exist.</returns>
        Task<string> GetEXAsync(string key, TimeSpan? expiry, CancellationToken cancellationToken = default);
        #endregion

    }
}
