namespace RedisClient.Abstractions
{
    public interface IRedisBasicOperator
    {
        public IRedisKeyOperator KeyOperator { get; }
        public IRedisStringOperator StringOperator { get; }
    }
}
