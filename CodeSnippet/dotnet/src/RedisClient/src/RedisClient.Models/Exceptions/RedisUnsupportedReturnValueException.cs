namespace RedisClient.Models.Exceptions
{
    public class RedisUnsupportedReturnValueException : RedisClientException
    {
        public RedisUnsupportedReturnValueException()
            : base() { }

        public RedisUnsupportedReturnValueException(string message)
            : base(message) { }

        public RedisUnsupportedReturnValueException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
