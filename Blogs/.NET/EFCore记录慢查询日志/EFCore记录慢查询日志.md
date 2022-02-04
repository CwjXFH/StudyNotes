在生产环境中，通常有DBA同事对数据库进行监控，在发现如慢查询等问题时反馈给开发团队进行解决。

.NET平台提供了[诊断机制](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md)，借助该机制可以实现EFCore记录慢查询日志功能，这样开发团队就可以通过日志告警发现慢查询问题而无需被动依赖DBA同事的反馈。

## 记录慢查询日志

基于.NET6创建API项目，安装[WJChi.Net.EFCoreSlowQuery](https://www.nuget.org/packages/WJChi.Net.EFCoreSlowQuery/)包，示例代码如下：

```c#
using Api.Database;
using EFCoreExtensions.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<InfoDbContext>(opt =>
{
    opt.UseSqlServer("Server = localhost;Database = Demo;User ID = sa;Password = Docker2022!;Application Name = EFCore;");
});


var app = builder.Build();

// Configure the HTTP request pipeline.

// Configuration via code
app.UseEFCoreSlowQuery(opt =>
{
    opt.ServiceName = "Demo APIs";
    opt.SlowQueryThresholdMilliseconds = 20;
});
app.MapControllers();

app.Run();
```

也支持通过配置文件进行配置：

```c#
builder.Services.Configure<EFCoreSlowQueryOptions>(builder.Configuration.GetSection(EFCoreSlowQueryOptions.OptionsName));
app.UseEFCoreSlowQuery();
```
配置文件内容如下：
```c#
{
    "EFCoreSlowQuery": {
        "ServiceName": "Demo APIs",
        "SlowQueryThresholdMilliseconds": 20
    }
}
```

输出如下：

![](.\EFCoreSlowQuery.png)



[点击这里](https://github.com/CwjXFH/StudyNotes/tree/master/CodeSnippet/dotnet/EFCoreUtils/samples/Api)可以查看完整示例。

## 推荐阅读

[How to identify slow running queries in SQL Server](https://www.sqlshack.com/how-to-identify-slow-running-queries-in-sql-server/)

[Overview of Logging and Interception](https://docs.microsoft.com/en-us/ef/core/logging-events-diagnostics/)

[DiagnosticSource User's Guide](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md)