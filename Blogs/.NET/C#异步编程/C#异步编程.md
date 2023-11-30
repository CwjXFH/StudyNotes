```c#
var d1 = new StructDemo();
d1.SetTag();
// 执行该方法后，d1._tag的值不会发生变化
// 异步方法状态机会拷贝d1到堆上，异步方法中对于字段的修改不会影响到变量d1
await d1.SetTagAsync(); 


file struct StructDemo
{
    private int _tag = 0;

    public StructDemo()
    {
    }

    public void SetTag()
    {
        _tag = 1;
    }

    public async Task SetTagAsync()
    {
        _tag = 100;
        await Task.CompletedTask;
    }
}
```



[c# - Struct's private field value is not updated using an async method - Stack Overflow](https://stackoverflow.com/questions/31642535/structs-private-field-value-is-not-updated-using-an-async-method)



Task.Yield作用

让渡控制权

[Consuming the Task-based Asynchronous Pattern - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern#configuring-suspension-and-resumption-with-yield-and-configureawait)



[.NET ThreadPool starvation, and how queuing makes it worse | by Kevin Gosse | Criteo R&D Blog | Medium](https://medium.com/criteo-engineering/net-threadpool-starvation-and-how-queuing-makes-it-worse-512c8d570527)