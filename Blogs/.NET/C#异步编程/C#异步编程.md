## Task状态机
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



## Task调度

```c#
using System.Diagnostics;


var sw = new Stopwatch();
sw.Start();
var tasks = new Task[3];
var maxSecs = tasks.Length;

// await LongDelay(tasks);
await ShortDelayAsync(tasks);

sw.Stop();
Console.WriteLine(sw.ElapsedMilliseconds);

Console.WriteLine();
return;

// 等待时间为所有Task等待时间的总和
async Task LongDelayAsync(Task[] inputTasks)
{
    for (var i = 0; i < inputTasks.Length; i++)
    {
        var secs = maxSecs - i;
        await SleepAsync(secs);
    }
}

// 等待时间为所有Task中的最长等待时间
async Task ShortDelayAsync(Task[] inputTasks)
{
    // 先调度
    for (var i = 0; i < inputTasks.Length; i++)
    {
        var secs = maxSecs - i;
        inputTasks[i] = SleepAsync(secs);
    }

    // 再等待
    foreach (var task in inputTasks)
    {
        await task;
    }
}

async Task SleepAsync(int secs)
{
    // return Task.Delay(secs * 1_000);
    await Task.Delay(secs * 1_000);
    Console.WriteLine($"等待：{secs}秒");
}
```

### 对比Python中的异步编程

```python
import asyncio
import time


async def block(secs):
    await asyncio.sleep(secs)


async def main():
    # 等待时间为所有task等待时间的总和
    # for i in range(1, 4):
    #     await asyncio.create_task(block(i))

    # 等待时间为所有task中的最长等待时间
    tasks = [asyncio.create_task(block(secs)) for secs in range(1, 4)]
    for task in tasks:
        await task


if __name__ == '__main__':
    start_time = int(time.time())
    asyncio.run(main())
    end_time = int(time.time())
    print(end_time - start_time)
```

Python中的异步模型是基于时间循环（event loop），通过task来调度协程（coroutines）。JavaScript中的异步编程与此类似，详情可参考：[异步与协程](../../JavaScript/异步与协程/异步与协程.md)。



Task.Yield作用

让渡控制权

[Consuming the Task-based Asynchronous Pattern - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern#configuring-suspension-and-resumption-with-yield-and-configureawait)



[.NET ThreadPool starvation, and how queuing makes it worse | by Kevin Gosse | Criteo R&D Blog | Medium](https://medium.com/criteo-engineering/net-threadpool-starvation-and-how-queuing-makes-it-worse-512c8d570527)