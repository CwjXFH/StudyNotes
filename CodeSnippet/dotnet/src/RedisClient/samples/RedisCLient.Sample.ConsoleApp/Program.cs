using Microsoft.Extensions.DependencyInjection;
using RedisClient.Abstractions;
using RedisClient.StackExchange.Extensions;
using RedisClient.Models.Options;

var serviceCollection = new ServiceCollection();
serviceCollection.Configure<RedisOptions>(opt =>
{
    opt.Host = "localhost";
    opt.Port = 6379;
});
serviceCollection.AddRedisClient();


using var serviceProvider = serviceCollection.BuildServiceProvider();
var basicOperator = serviceProvider.GetService<IRedisBasicOperator>();

// Lazy
//Console.WriteLine(basicOperator!.StringOperator);
//Console.WriteLine(basicOperator.StringOperator);
//Console.WriteLine(basicOperator.StringOperator == basicOperator.StringOperator);
////Console.WriteLine(basicOperator.KeyOperator);
////Console.WriteLine(basicOperator.KeyOperator);

await basicOperator!.StringOperator.SetAsync("key", "value", TimeSpan.FromMilliseconds(200));
Console.WriteLine(await basicOperator.StringOperator.GetAsync("key", CancellationToken.None));
Console.WriteLine(await basicOperator.KeyOperator.ExistsAsync("key", CancellationToken.None));
Thread.Sleep(200);
Console.WriteLine(await basicOperator.KeyOperator.ExistsAsync("key", CancellationToken.None));

await basicOperator.StringOperator.SetRangeAsync("key", 1, "abc");
var val = basicOperator.StringOperator.GetAsync("key", CancellationToken.None);
Console.WriteLine(val);
