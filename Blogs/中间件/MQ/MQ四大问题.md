MQ四大问题：

+ 如何防止消息丢失
+ 如何处理重复消费
+ 如何解决消息堆积
+ 特定场景消息顺序



## 消息丢失

通常通过以下三个方面来确保消息不丢失

#### 发送方确认

>  有些队列提供了事物的功能，来确保发送过程不丢失消息，不过事务对性能损耗太大，不推荐使用。
>
> RabbitMQ事务  vs Kafka事务

broker在收到发送方发送的消息后，可以返回给发送者一个确认响应，发送者收到响应后可以认为消息发送成功。

若发送方发送消息失败或者接受broker确认超时，可以进行重试，达到最大重试次数后依然无法发送成功则需记录日志，失败次数达到一定阈值时可以发送告警信息来告知开发者系统故障。

关于broker何种情况下回返回确认，请参考：

+ [when-publishes-are-confirmed — RabbitMQ](https://www.rabbitmq.com/confirms.html#when-publishes-are-confirmed)

  对于无法路由到具体队列的消息直接返回确认;对于可路由的消息，则在消息成功分发到每个匹配队列后返回确认，即，对于持久化消息在写入磁盘后返回确认、对于Quorum queue，消息成功分发到每个queue后返回确认

+ kafka发送方确认

#### Broker存储

>  冗余与分散是高可用的不二法门

消息持久化到磁盘，分为逐条持久化和批量持久化

消费分发到多个队列，Kafka中的领导者和跟随者分区，RabbitMQ中的Quorum queue或消息分发到多个队列

#### 消费者确认

在执行完具体业务逻辑后再返回确认信息到borker，不要自动确认。

> RabbitMQ中的消费者自动确认机制[无法保证消息不丢失](https://www.rabbitmq.com/confirms.html#acknowledgement-modes)

#### 发件箱模式

[Outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html)，发件箱模式借助本地事务可以确保消息不丢失，同时也可以作为实现分布式事务的一种方式（~~最大努力通知~~/可靠消息投递）。


## 重复消费





[幂等性](../../分布式/幂等性.md)

## 消息堆积







## 推荐阅读

[MQ那点破事！消息丢失、重复消费、消费顺序、堆积、事务、高可用.... ](https://database.51cto.com/art/202109/684263.htm)

[奈何花开 - InfoQ](https://www.infoq.cn/profile/BF112BC8BF6889/publish?menu=)

[Consumer Acknowledgements and Publisher Confirms — RabbitMQ](https://www.rabbitmq.com/confirms.html)

[FAQ: How to Optimize the RabbitMQ Prefetch Count - CloudAMQP](https://www.cloudamqp.com/blog/how-to-optimize-the-rabbitmq-prefetch-count.html)