`pip install redis`



## pub/sub

消息发送到channel，和具体的Redis库（默认一个Redis实例有16个库）没有关系

集群环境下和具体的节点无关，连接到任一个节点都可以获取消息

可以发消息给多个订阅者

channel支持模式匹配

消息不会被持久化

没有消费者确认机制

消息无法重新入队再次分发



Redis宕机后消息丢失

消息积压超过缓冲区大小会强制下线消费者，导致消息丢失



断线重连后，无法获取断线期间发送的消息

没有消费者连接到channel上，则发送的消息全部丢失

```python
# _*_ coding=utf8 _*_

import time
import redis

r = redis.Redis(host='')

# region pub/sub
ps = r.pubsub()
# ps.subscribe(im=lambda msg: print(f'receive {msg["data"]} from {msg["channel"]}'))
im_callback = {'im*': lambda msg: print(f'receive {msg["data"]} from {msg["channel"]}')}
ps.psubscribe(**im_callback)
while True:
    msg = ps.get_message()
    if msg:
        print(f'print msg: {msg}')
    time.sleep(0.1)
# for msg in ps.listen():
#     print(msg)
ps.close()
# endregion

r.close()
```

> 参考：[Pub/Sub – Redis](https://redis.io/topics/pubsub)



## List

消息只能发给一个订阅者



没有消费者确认机制

消息无法重新入队再次分发



消费者消费消息期间宕机会导致消息丢失

消息可以持久化，但Redis持久化策略决定**不能保证消息不丢失**

```python
# r.lpush('msg', 'hi')
while True:
    # msg = r.rpop('msg')
    # 阻塞式拉取消息
    msg = r.brpop('msg')
    if msg:
        print(msg)
        # r.lpush('msg', msg[1])
    # time.sleep(0.1)
```



> 参考：[颠覆认知——Redis会遇到的15个「坑」，你踩过几个？](https://mp.weixin.qq.com/s/xIEVj5oJ7rvEMWB19SHBwA) **AOF everysec 真的只会丢失 1 秒数据？**部分



## ZSet



## Stream



支持消费者确认

消息堆积超过队列长度时会丢弃旧消息

消息可以持久化，但Redis持久化策略决定**不能保证消息不丢失**

## 小结

Redis中的List属于拉模型，pub/sub属于推模型，两者都无法保证消息不丢失，不可重复消费消息，也不支持消费者确认

为什么要用Redis作为队列而不是使用RabbitMQ或者Kafka？

+ 系统架构设计遵循简单够用易扩展原则，一些对数据丢失不敏感或者使用需求不复杂的场景下可以使用Redis最为队列，同时对收发消息做一层抽象利于日后业务变复杂时替换为其他消息中间件
+ 少依赖一个中间件利于降低系统复杂度、提升系统稳定性



## 推荐阅读

[把 Redis 当作队列用，真的合适吗？](https://mp.weixin.qq.com/s/l1dMnu6laOLm375mMxy3WQ)