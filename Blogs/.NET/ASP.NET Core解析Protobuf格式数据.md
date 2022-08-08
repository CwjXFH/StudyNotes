ASP.NET Core通过[IInputFormatter](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.formatters.iinputformatter?view=aspnetcore-6.0)来解析输入的数据，并进行模型绑定（Model Binding）；通过[IOutputFormatter](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.formatters.ioutputformatter?view=aspnetcore-6.0)来解析输出的数据，来格式化响应（format response）。



## ASP.NET Core解析Protocol Buffers

ASP.NET Core默认只支持对`application/json`的解析，要解析protobuf格式数据，需要引入nuget包：[AspCoreProtobufFormatters](https://github.com/jamcar23/AspCoreProtobufFormatters)，该包`1.0.0`版本默认不支持`application/json`格式，可以通过扩展来支持：

```c#
internal static class HttpContentType
{
    public static class Application
    {
        public const string Json = "application/json";
    }
}


/// <summary>
/// 针对ContentType为<see cref="HttpContentType.Application.Json"/>类型数据的格式化器
/// </summary>
internal class ProtobufApplicationJsonFormatter : ProtobufJsonFormatter
{
    public ProtobufApplicationJsonFormatter() : base(HttpContentType.Application.Json) { }

    protected override (bool, byte[]) WriteBytes(IMessage message)
    => (true, Encoding.UTF8.GetBytes(ProtoModelTypeRegister.JsonFormatter.Format(message)));
}
```

在ASP.NET Core中添加引用：

```c#
builder.Services.AddControllers(opt =>
{
    opt.AddProtobufFormatters(new IContentReader[] { new ProtobufBinFormatter(), new ProtobufApplicationJsonFormatter() },
                              new IContentWriter[] { new ProtobufBinFormatter(), new ProtobufApplicationJsonFormatter() });
});
```

注意，这里添加formatter时，Protobuf formatter在前，json formatter在后，所以会优先选用protobuf formatter来格式化数据。如果想要返回json格式数据，可以根据内容协商机制在Accept头字段中指定`application/json`。对于不支持内容协商的场景，可以通过自定义一个过滤器来实现：

```c#
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class EnableJsonResponseFilterAttribute : ResultFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.HttpContext.Request.AcceptJson())
        {
            // 执行response format之前将response content-type设为json格式
            context.HttpContext.Response.ContentType = context.HttpContext.Request.ContentType!;
        }
    }
}

internal static class HttpExtensions
{
    /// <summary>
    /// 判断当前HTTP请求的Content-Type中是否包含 <see cref="HttpContentType.Application.Json"/>
    /// </summary>
    public static bool ContentTypeIsJson(this HttpRequest request)
    {
        foreach (var contentType in request.Headers.ContentType)
        {
            if (contentType.Contains(HttpContentType.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断当前HTTP请求是否接受Json格式的返回数据，目前只通过Content-Type来判断，忽略Accept
    /// </summary>
    public static bool AcceptJson(this HttpRequest request)
    {
        foreach (var accept in request.Headers.Accept)
        {
            if (accept.Contains(HttpContentType.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return ContentTypeIsJson(request);
    }
}
```



## 两个Nuget包

谷歌提供了[Google.Protobuf](https://www.nuget.org/packages/Google.Protobuf)包用于解析Protocol Buffers数据，包括和json格式互转；[Grpc.Tools](https://www.nuget.org/packages/Grpc.Tools/#readme-body-tab)包可根据proto文件在编译时生成对应的c#/c++文件。



## 推荐阅读

[Protocol Buffers](https://developers.google.com/protocol-buffers)  

[Custom formatters in ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/custom-formatters?view=aspnetcore-6.0)  

[Format response data in ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-6.0)  