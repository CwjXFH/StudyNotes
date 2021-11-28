namespace RedisClient.Models.Exceptions
{
    public class RedisClientException : Exception
    {
        public RedisClientException()
            : base() { }

        public RedisClientException(string message)
            : base(message) { }

        public RedisClientException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
