using Microsoft.Extensions.Options;
using RedisClient.Abstractions;
using RedisClient.Models.Options;
using StackExchange.Redis;

namespace RedisClient.Internal
{
    internal class RedisConnectionFactory : IRedisConnectionFactory<ConnectionMultiplexer>, IDisposable
    {
        private SemaphoreSlim createConnSemaphore = new SemaphoreSlim(1, 1);

        private ConnectionMultiplexer? redisConnection;

        private readonly IOptionsMonitor<RedisOptions> optionsMonitor;

        public RedisConnectionFactory(IOptionsMonitor<RedisOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
            this.optionsMonitor.OnChange(async opt =>
            {
                await createConnSemaphore.WaitAsync();
                DisposeConnection();
                createConnSemaphore.Release();
            });
        }

        public async Task<ConnectionMultiplexer> CreateAsync(CancellationToken cancellationToken = default)
        {
            if (redisConnection != null)
            {
                return redisConnection;
            }

            await createConnSemaphore.WaitAsync();
            var options = optionsMonitor.CurrentValue;
            redisConnection = await ConnectionMultiplexer.ConnectAsync($"{options.Host}:{options.Port}", opt =>
            {
                opt.Password = options.Password;
            });
            createConnSemaphore.Release();

            return redisConnection;
        }

        public ConnectionMultiplexer Create()
        {
            if (redisConnection != null)
            {
                return redisConnection;
            }

            createConnSemaphore.Wait();
            var options = optionsMonitor.CurrentValue;
            redisConnection = ConnectionMultiplexer.Connect($"{options.Host}:{options.Port}", opt =>
            {
                opt.Password = options.Password;
            });
            createConnSemaphore.Release();

            return redisConnection;
        }

        public void Dispose()
        {
            DisposeConnection();
        }

        private void DisposeConnection()
        {
            redisConnection?.Dispose();
        }
    }
}
