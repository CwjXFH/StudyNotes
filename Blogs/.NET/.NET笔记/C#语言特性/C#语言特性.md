## new和override的区别

<img src="./imgs/new_vs_override.png" />

> new调用哪个方法看变量编译时类型，override看运行时类型



## 委托涉及的内存分配

[Understanding the cost of C# delegates](https://devblogs.microsoft.com/dotnet/understanding-the-cost-of-csharp-delegates/)



> 尽可能使用静态，避免出现闭包



## 按引用传递及可能导致的性能问题

[Performance traps of ref locals and ref returns in C#](https://devblogs.microsoft.com/premier-developer/performance-traps-of-ref-locals-and-ref-returns-in-c/)

注意点：

+ **数组索引器按照引用返回**，List集合索引器按照值返回

+ 按引用传递不会复制结构体，按值传递会复制结构体

+ 对于使用readonly修饰的非只读结构体类型变量，在每次访问其成员时，都会创建**防御性副本**，即使使用`ref readonly`

+ readonly成员访问非readonly成员会导致复制（结合上一条）

+ 按引用返回会破坏封装性，因为调用方获得了完全控制权

+ 在方法调用结束后依然基于别名使用方法中分配在栈上的变量会导致应用崩溃

  所以，不能按照引用返回局部变量；在结构体中不能按引用返回this，详情参考：[Ref safe context](https://learn.microsoft.com/zh-cn/dotnet/csharp/advanced-topics/performance/#ref-safe-context)



#### 按值传递结构体的坑

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

> [c# - Struct's private field value is not updated using an async method - Stack Overflow](https://stackoverflow.com/questions/31642535/structs-private-field-value-is-not-updated-using-an-async-method)



#### 数组索引器按引用返回

```c#
// List<Demo> darr = [new Demo(100), new Demo(200)];

Demo[] darr = [new Demo(100), new Demo(200)];

var d = darr[0];
Console.WriteLine(d.Count);
d.Add();
Console.WriteLine(d.Count);

Console.WriteLine("直接访问数组元素");

Console.WriteLine(darr[0].Count);
darr[0].Add();
Console.WriteLine(darr[0].Count);  // darr是数组时和是List<>类型时输出结果不一样
Console.WriteLine();

struct Demo(int count)
{
    public int Count => count;

    public void Add() => count++;
}
```



#### readonly防御性复制

```c#
public struct FairlyLargeStruct
{
    private readonly long l1, l2, l3, l4;
    public int N { get; }
    public FairlyLargeStruct(int n) : this() => N = n;
}


==================================================================================


private FairlyLargeStruct _nonReadOnlyStruct = new FairlyLargeStruct(42);
// 每次访问结构体成员都会导致防御性复制
private readonly FairlyLargeStruct _readOnlyStruct = new FairlyLargeStruct(42);
private readonly int[] _data = Enumerable.Range(1, 100_000).ToArray();
        
[Benchmark]
public int AggregateForNonReadOnlyField()
{
    int result = 0;
    foreach (int n in _data)
        result += n + _nonReadOnlyStruct.N;
    return result;
}

[Benchmark]
public int AggregateForReadOnlyField()
{
    int result = 0;
    foreach (int n in _data)
        result += n + _readOnlyStruct.N;
    return result;
}
```



#### `ref readonly`防御性复制

```c#
public struct BigStruct
{
    // Other fields
    public int X { get; }
    public int Y { get; }
}
 
private BigStruct _bigStruct;
public ref readonly BigStruct GetBigStructByRef() => ref _bigStruct;
 
ref readonly var bigStruct = ref GetBigStructByRef();
int result = bigStruct.X + bigStruct.Y;
```

#### readonly成员访问非readonly成员会导致复制

readonly 成员可以调用结构体的非 readonly 成员。当这样调用时，编译器会创建一个结构体实例的副本，然后在这个副本上调用非 readonly 成员。这意味着任何由非 readonly 成员进行的修改只会影响副本，而不会影响结构体的原始实例。

```c#
var item = new MyStruct { Value = 666 };
item.ReadonlyMethod();
Console.WriteLine(item.Value);  // 666

item.NonReadonlyMethod();
Console.WriteLine(item.Value);  // 10

public struct MyStruct
{
    public int Value;

    public readonly void ReadonlyMethod()
    {
        // 这行代码会导致编译错误：
        // Value = 10; // 无法为 'Value' 赋值，因为它是 readonly 实例成员

        // 然而，调用非 readonly 方法是允许的：
        NonReadonlyMethod();
    }

    public void NonReadonlyMethod()
    {
        // 这个方法可以修改结构体的字段
        Value = 10;
    }
}
```
