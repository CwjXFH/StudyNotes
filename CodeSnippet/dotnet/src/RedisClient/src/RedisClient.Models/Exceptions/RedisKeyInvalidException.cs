namespace RedisClient.Models.Exceptions
{
    public class RedisKeyInvalidException : RedisClientException
    {
        public RedisKeyInvalidException()
            : base() { }

        public RedisKeyInvalidException(string message)
            : base(message) { }

        public RedisKeyInvalidException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
