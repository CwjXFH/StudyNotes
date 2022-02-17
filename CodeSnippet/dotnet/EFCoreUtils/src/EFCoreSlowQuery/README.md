## Summary

This is a middleware used for log EFCore slow query.  

## Release notes

Click [here](https://github.com/CwjXFH/StudyNotes/blob/master/CodeSnippet/dotnet/EFCoreUtils/src/EFCoreSlowQuery/RELEASE-NOTES.md) for release notes.

## How to use

A complete example can be found [here](https://github.com/CwjXFH/StudyNotes/tree/master/CodeSnippet/dotnet/EFCoreUtils/samples/Api).

```c#
using Api.Database;
using EFCoreExtensions;
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
    opt.ServiceName = "DemoApi";
    opt.SlowQueryThresholdMilliseconds = 20;
});
app.MapControllers();

app.Run();
```
Also support configure via configuration file:  
```json
{
    "EFCoreSlowQuery": {
        "ServiceName": "DemoApi",
        "SlowQueryThresholdMilliseconds": 20
    }
}
```

v0.0.3
```c#
// will get configuration from config file
app.UseEFCoreSlowQuery();
```

v0.0.1/v0.0.2
```c#
builder.Services.Configure<EFCoreSlowQueryOptions>(builder.Configuration.GetSection(EFCoreSlowQueryOptions.OptionsName));
app.UseEFCoreSlowQuery();
```
