## 配置提供程序

VolatileConfigurationProvider:

```c#
using System.Collections;
using Microsoft.Extensions.Configuration;

namespace Libraries.AspNetCoreExtensions.Extensions.Configuration;

/// <summary>
/// 非持久化配置，类似<see cref="Microsoft.Extensions.Configuration.Memory.MemoryConfigurationProvider"/>
/// </summary>
public class VolatileConfigurationProvider : ConfigurationProvider, IEnumerable<KeyValuePair<string, string?>>
{
    private readonly VolatileConfigurationSource _cfgSource;

    public VolatileConfigurationProvider(VolatileConfigurationSource cfgSource)
    {
        _cfgSource = cfgSource;
    }

    public void Add(string key, string? value)
    {
        Data.Add(key, value);
    }

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
```

VolatileConfigurationSource:

```c#
using Microsoft.Extensions.Configuration;

namespace Libraries.AspNetCoreExtensions.Extensions.Configuration;

/// <summary>
/// 非持久化配置，类似<see cref="Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource"/>
/// </summary>
public class VolatileConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new VolatileConfigurationProvider(this);
}
```

VolatileConfigurationSourceExtensions:

```c#
using Microsoft.Extensions.Configuration;

namespace Libraries.AspNetCoreExtensions.Extensions.Configuration;

public static class VolatileConfigurationSourceExtensions
{
    private static bool _registered;
    public static IConfigurationBuilder TryAddVolatileConfigurationSource(this IConfigurationBuilder configurationBuilder)
    {
        if (_registered == false)
        {
            configurationBuilder.Add(new VolatileConfigurationSource());
            _registered = true;
        }

        return configurationBuilder;
    }
}
```



## 鉴权

TokenAuthenticationHandler:

```c#
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Libraries.AspNetCoreExtensions.Extensions.Auth;

/// <summary>
/// 简单的Token认证处理器
/// </summary>
/// <example>
/// HTTP请求示例
/// GET http://localhost:80
/// Authorization: token
/// </example>
public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationSchemeOptions>
{
    private readonly IOptionsMonitor<TokenAuthOptions> _optionsMonitor;

    public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthOptions> optionsMonitor, IOptionsMonitor<TokenAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _optionsMonitor = optionsMonitor;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue("Authorization", out var token) == false)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var validToken = _optionsMonitor.CurrentValue.Token;
        if (validToken is { Length: > 0 } && token == validToken)
        {
            var claim = new Claim("Token", token!);
            // 这里将authentication type设为Basic，但实际上这里不是标准的Basic认证
            var identity = new ClaimsIdentity(new[] { claim }, "Basic");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        var realm = $"""
                    Basic realm={Request.Path}
                    """;
        Response.Headers.Add("WWW-Authenticate", realm);
        return Task.FromResult(AuthenticateResult.Fail("无效的认证信息"));
    }
}

```

TokenAuthenticationOptions:

```c#
using Microsoft.AspNetCore.Authentication;

namespace Libraries.AspNetCoreExtensions.Extensions.Auth;

public class TokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string SchemaName = "TokenAuthSchema";
}

/// <summary>
/// 用户配置项
/// </summary>
public class TokenAuthOptions
{
    public const string SectionName = "TokenAuth";
    public string Token { set; get; } = "";
}
```

TokenAuthExtensions:

```c#
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Libraries.AspNetCoreExtensions.Extensions.Auth;

public static class TokenAuthExtensions
{
    public static AuthenticationBuilder AddTokenAuthentication(this AuthenticationBuilder auth, IConfiguration configuration)
    {
        auth.Services.Configure<TokenAuthOptions>(configuration.GetSection(TokenAuthOptions.SectionName));
        auth.AddScheme<TokenAuthenticationSchemeOptions, TokenAuthenticationHandler>(TokenAuthenticationSchemeOptions.SchemaName, null);
        return auth;
    }
}
```




## API

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();
// 注册顺序位于默认ConfigurationSource之后，以获取更高的优先级
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#configuration-providers
builder.Configuration.TryAddVolatileConfigurationSource();

builder.Services
    .AddHttpClient()
    .AddControllers();
builder.Services.AddAuthentication().AddTokenAuthentication(builder.Configuration);

var host = builder.Build();
host.UseMQConsumers();
host.MapControllers();
await host.RunAsync();
```




```c#
using Libraries.AspNetCoreExtensions.Extensions.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Libraries.AspNetCoreExtensions.Controllers;

[Route("[controller]")]
[Authorize(AuthenticationSchemes = TokenAuthenticationSchemeOptions.SchemaName)]
public class ConfigureControllerBase : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigureControllerBase(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("[action]")]
    public ActionResult Change(string key, string value)
    {
        var cfgKey = key.Replace("__", ":");
        _configuration[cfgKey] = value;
        // Reload重建缓存，不会改变配置源原有的数据，即，Reload前后配置体系中的数据没有变化
        // 如：appsettings.json重新加载后还是旧数据，除非更改了文件内容，但FileConfigurationProvider并不会改变文件本身的内容
        // VolatileConfigurationSource中会保留设置的值，且优先级较高（或者说最后被读取，所以可以覆盖前面的相同key的值），配置体系会从中获取到设置的新值
        if (_configuration is IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
        }

        var cfgVal = _configuration[cfgKey];
        return Ok(cfgVal ?? "null");
    }

    [AllowAnonymous]
    public ActionResult Get(string key)
    {
        var cfgKey = key.Replace("__", ":");
        var cfgVal = _configuration[cfgKey];
        return Ok(cfgVal ?? "null");
    }
}
```

appsettings.json文件：

```json
{  
  "TokenAuth": {
    "Token": "a5df1f91-efb8-47d9-a70a-9f7c66ba70f5"
  }
}
```

