`pip install redis`



## pub/sub

消息发送到channel，和具体的库没有关系

集群环境下和具体的节点无关，连接到任一个节点都可以获取消息

可以发消息给多个订阅者

channel支持模式匹配

消息不会被持久化

断线重连后，无法获取断线期间发送的消息

消息无法重新入队再次分发

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

消息可以持久化

Redis持久化策略决定**不能保证消息不丢失**

消息无法重新入对再次分发



> 参考：[颠覆认知——Redis会遇到的15个「坑」，你踩过几个？](https://mp.weixin.qq.com/s/xIEVj5oJ7rvEMWB19SHBwA) **AOF everysec 真的只会丢失 1 秒数据？**部分



## ZSet



## Stream





## 小结

为什么要用Redis作为队列而不是使用RabbitMQ或者Kafka？

+ 系统架构设计遵循简单够用易扩展原则，一些对数据丢失不敏感或者使用需求不复杂的场景下可以使用Redis最为队列，同时对首发消息做一层抽象利于日后业务变复杂时替换为其他消息中间件
+ 少依赖一个中间件利于降低系统复杂度、提升系统稳定性



