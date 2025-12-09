```python
import asyncio
import time


async def block(secs):
    await asyncio.sleep(secs)


async def main():
    # 等待时间为所有task等待时间的总和
    # for i in range(1, 4):
    #     await asyncio.create_task(block(i))

    # 先创建出所有的task，等待时间为所有task中的最长等待时间
    tasks = [asyncio.create_task(block(secs)) for secs in range(1, 4)]
    for task in tasks:
        await task


if __name__ == '__main__':
    start_time = int(time.time())
    asyncio.run(main())
    end_time = int(time.time())
    print(end_time - start_time)
```

Python中的异步模型是基于事件循环（event loop），通过task来调度协程（coroutines）。JavaScript中的异步编程与此类似，详情可参考：[异步与协程](../JavaScript/异步与协程/异步与协程.md)。



```python
from concurrent import futures
# import asyncio.futures


# 多进程（并行）
def concurrent():
    with futures.ProcessPoolExecutor() as executor:
        future = executor.submit(block, 1)

      
# 多线程
def concurrent2():
    with futures.ThreadPoolExecutor(max_workers=5) as executor:
        future = executor.submit(block, 2)
```

Python中的futures，可对比JavaScript中的Promise



I/O密集型使用多线程或者更轻量级的协程；

CPU密集型使用多进程；



event loop、task、coroutine演示：

coroutine转换为task，event loop调度task、遇到I/O阻塞控制权返回给event loop

future是更底层的对象，用于存放执行结果

```python
import asyncio


async def print_hi(second: int, msg: str):
    print(f"ready say hi {msg}")
    await asyncio.sleep(second)
    print(f"hi {msg}")


# async def main():
#     t1 = asyncio.create_task(print_hi(2, "t1"))
#     t2 = asyncio.create_task(print_hi(1, "t2"))
#     await t1
#     await t2
#     print("main done")

async def main():
    await asyncio.gather(
        print_hi(2, "t1"),
        print_hi(1, "t2"),
    )
    print("main done")


asyncio.run(main())

# 下面代码演示asyncio.run()的基本原理
# event_loop = asyncio.new_event_loop()
#
# main_task = event_loop.create_task(main())
# main_task.add_done_callback(lambda t: print("main done"))
#
# event_loop.run_until_complete(main_task)
```



## 推荐阅读

[Coroutines and Tasks — Python 3.14.2 documentation](https://docs.python.org/3.14/library/asyncio-task.html)