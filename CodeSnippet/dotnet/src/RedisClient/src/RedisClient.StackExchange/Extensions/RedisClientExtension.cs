using Microsoft.Extensions.DependencyInjection;
using RedisClient.Abstractions;
using RedisClient.Models.Options;
using RedisClient.StackExchange.Internal;
using StackExchange.Redis;

namespace RedisClient.StackExchange.Extensions
{
    public static partial class RedisBasicOperatorExtension
    {
        public static void AddRedisClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<RedisOptions>();

            serviceCollection.AddSingleton<IRedisConnectionFactory<ConnectionMultiplexer>, RedisConnectionFactory>();
            serviceCollection.AddScoped<IRedisBasicOperator, RedisBasicOperator>();
        }
    }
}
