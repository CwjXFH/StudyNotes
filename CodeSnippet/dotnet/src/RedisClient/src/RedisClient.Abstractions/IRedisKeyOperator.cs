namespace RedisClient.Abstractions
{
    public interface IRedisKeyOperator
    {
        Task<bool> ExistsAsync(string key);
        Task<bool> DeleteAsync(string key);
    }
}
