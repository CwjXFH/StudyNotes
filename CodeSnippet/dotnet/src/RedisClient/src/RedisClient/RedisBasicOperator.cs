using Microsoft.Extensions.Options;
using RedisClient.Abstractions;
using RedisClient.Internal;
using RedisClient.Models.Options;
using StackExchange.Redis;

namespace RedisClient
{
    public class RedisBasicOperator : IRedisBasicOperator
    {
        private IOptionsMonitor<RedisOptions> optionsMonitor;
        private IRedisConnectionFactory<ConnectionMultiplexer> connectionFactory;

        private readonly IContravariantLazy<IRedisStringOperator> stringOperator;
        private readonly IContravariantLazy<IRedisKeyOperator> keyOperator;

        public RedisBasicOperator(IOptionsMonitor<RedisOptions> optionsMonitor, IRedisConnectionFactory<ConnectionMultiplexer> connectionFactory)
        {
            this.optionsMonitor = optionsMonitor;
            this.connectionFactory = connectionFactory;

            stringOperator = CreateOperator<RedisStringOperator>();
            keyOperator = CreateOperator<RedisKeyOperator>();
        }

        public IRedisKeyOperator KeyOperator => keyOperator.Value;

        public IRedisStringOperator StringOperator => stringOperator.Value;


        #region Lazy
        private IContravariantLazy<TOperator> CreateOperator<TOperator>() where TOperator : RedisOperator
            => new MyLazy<TOperator>(() =>
            {
                var conn = connectionFactory.Create();
                var db = conn.GetDatabase(optionsMonitor.CurrentValue.DbIndex);
                return (Activator.CreateInstance(typeof(TOperator), db) as TOperator)!;
            }, true);

        private interface IContravariantLazy<out T>
        {
            T Value { get; }
        }

        private class MyLazy<T> : Lazy<T>, IContravariantLazy<T>
        {
            public MyLazy(Func<T> func, bool isThreadSafe)
                : base(func, isThreadSafe) { }

            public new T Value => base.Value;
        }
        #endregion
    }
}
