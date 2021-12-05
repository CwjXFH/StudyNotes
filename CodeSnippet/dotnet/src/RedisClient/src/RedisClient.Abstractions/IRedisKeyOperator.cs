namespace RedisClient.Abstractions
{
    public interface IRedisKeyOperator
    {
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    }
}
