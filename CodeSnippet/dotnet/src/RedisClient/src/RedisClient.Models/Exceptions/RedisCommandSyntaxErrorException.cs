namespace RedisClient.Models.Exceptions
{
    public class RedisCommandSyntaxErrorException : RedisClientException
    {
        public RedisCommandSyntaxErrorException()
            : base() { }

        public RedisCommandSyntaxErrorException(string message)
            : base(message) { }

        public RedisCommandSyntaxErrorException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
