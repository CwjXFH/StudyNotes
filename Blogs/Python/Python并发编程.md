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

Python中的异步模型是基于时间循环（event loop），通过task来调度协程（coroutines）。JavaScript中的异步编程与此类似，详情可参考：[异步与协程](../JavaScript/异步与协程/异步与协程.md)。



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