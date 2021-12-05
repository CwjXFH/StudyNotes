using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisClient.Abstractions
{
    public interface IRedisHashOperator
    {
        Task<bool> HSetAsync(string key, IDictionary<string, string> value, CancellationToken cancellationToken = default);
    }
}
