using RedisClient.Models.Enums;

namespace RedisClient.Abstractions
{
    /// <summary>
    /// Operate redis string type
    /// </summary>
    /// <seealso href="https://redis.io/commands#string"/>
    public interface IRedisStringOperator
    {
        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. 
        /// Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="expiry">Set the specified expire time.</param>
        /// <param name="writeBehavior"><see cref="KeyWriteBehavior"/></param>
        /// <returns>True if value was set, otherwise false.</returns>
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None, CancellationToken cancellationToken = default);
        /// <summary>
        /// Set key to hold the value. If key already holds a value, it is overwritten, regardless of its type.
        /// Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="expiry">Set the specified expire time.</param>
        /// <param name="writeBehavior"><see cref="KeyWriteBehavior"/></param>
        /// <returns>True if value was set, otherwise false.</returns>
        /// <exception cref="System.NotSupportedException">
        /// There is no compatible System.Text.Json.Serialization.JsonConverter for TValue  or its serializable members.
        /// </exception>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry, KeyWriteBehavior writeBehavior = KeyWriteBehavior.None, CancellationToken cancellationToken = default);
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
    }
}
