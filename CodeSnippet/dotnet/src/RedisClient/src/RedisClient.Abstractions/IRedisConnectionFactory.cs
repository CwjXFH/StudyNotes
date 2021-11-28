namespace RedisClient.Abstractions
{
    public interface IRedisConnectionFactory<TConn>
        where TConn : class
    {
        Task<TConn> CreateAsync();
        TConn Create();
    }
}
