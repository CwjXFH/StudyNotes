using Microsoft.Extensions.DependencyInjection;
using RedisClient.Abstractions;
using RedisClient.StackExchange.Extensions;
using RedisClient.Models.Options;
using RedisClient.Models.Enums;
using StackExchange.Redis;

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

//await basicOperator!.StringOperator.SetAsync("key", "value", TimeSpan.FromMilliseconds(200));
//Console.WriteLine(await basicOperator.StringOperator.GetAsync("key", CancellationToken.None));
//Console.WriteLine(await basicOperator.KeyOperator.ExistsAsync("key", CancellationToken.None));
//Thread.Sleep(200);
//Console.WriteLine(await basicOperator.KeyOperator.ExistsAsync("key", CancellationToken.None));

//await basicOperator.StringOperator.SetRangeAsync("key", 1, "abc");
//var val = basicOperator.StringOperator.GetAsync("key", CancellationToken.None);
//Console.WriteLine(val);

var msetnxResult = await basicOperator.StringOperator.MSetNXAsync(new Dictionary<string, string>() { ["1"] = "1" });
Console.WriteLine(msetnxResult);
//msetnxResult = await basicOperator.StringOperator.MSetNXAsync(new Dictionary<string, string>() { ["1"] = "1", ["2"] = "2" });
//Console.WriteLine(msetnxResult);
////await basicOperator.StringOperator.MSetAsync(new Dictionary<string, string>() { ["1"] = "1", ["2"] = "2" });
//var mgetResult = await basicOperator.StringOperator.MGetAsync(new[] { "1", "2" });
//Console.WriteLine(mgetResult.Data.Count);
var getexResult = await basicOperator.StringOperator.GetEXAsync("1", TimeSpan.FromSeconds(100));
Console.WriteLine(getexResult);

//var oldVal = await basicOperator.StringOperator.SetAsync("100", "1val", writeBehavior: KeyWriteBehavior.None, returnOldValue: false);
//Console.WriteLine(oldVal.Successed);
