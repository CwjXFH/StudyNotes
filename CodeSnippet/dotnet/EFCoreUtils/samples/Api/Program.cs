using Api.Database;
using EFCoreSlowQuery;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<InfoDbContext>(opt =>
{
    opt.UseSqlServer("Server = localhost;Database = Demo;User ID = sa;Password = Docker2022!;Application Name = EFCore;");
});
builder.Services.Configure<EFCoreSlowQueryOptions>(builder.Configuration.GetSection(EFCoreSlowQueryOptions.OptionsName));


var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure via configuration file
//app.UseEFCoreSlowQuery();
// Configuration via code
app.UseEFCoreSlowQuery(opt =>
{
    opt.ServiceName = "DemoApi2";
    opt.SlowQueryThresholdMilliseconds = 20;
});
app.MapControllers();

app.Run();
