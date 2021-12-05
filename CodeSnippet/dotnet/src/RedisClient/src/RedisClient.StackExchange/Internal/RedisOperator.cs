using RedisClient.Models.Exceptions;
using StackExchange.Redis;

namespace RedisClient.StackExchange.Internal
{
    internal class RedisOperator
    {
        protected readonly IDatabase database;

        public RedisOperator(IDatabase database)
        {
            this.database = database;

        }

        protected void ThrowIfKeyInvalid(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new RedisKeyInvalidException("invalid key");
            }
        }
    }
}
