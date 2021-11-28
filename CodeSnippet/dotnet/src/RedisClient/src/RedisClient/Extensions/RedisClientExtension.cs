using Microsoft.Extensions.DependencyInjection;
using RedisClient.Abstractions;
using RedisClient.Internal;
using RedisClient.Models.Options;
using StackExchange.Redis;

namespace RedisClient.Extensions
{
    public static class RedisClientExtension
    {
        public static void AddRedisClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<RedisOptions>();

            serviceCollection.AddSingleton<IRedisConnectionFactory<ConnectionMultiplexer>, RedisConnectionFactory>();
            serviceCollection.AddScoped<IRedisBasicOperator, RedisBasicOperator>();
        }
    }
}
