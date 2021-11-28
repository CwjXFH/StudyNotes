namespace RedisClient.Abstractions
{
    public interface IRedisStringOperator
    {
        Task<bool> SetAsync(string key, string value);
        Task<bool> SetAsync(string key, string value, TimeSpan expiry);

        Task<string> GetAsync(string key);
        Task<T> GetAsync<T>(string key, T defaultValue);
    }
}
