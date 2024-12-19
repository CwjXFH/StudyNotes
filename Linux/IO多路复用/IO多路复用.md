#### 什么是I/O多路复用？

使用少量的线程处理大量的I/O操作（网路I/O，磁盘I/O）

#### 为什么需要I/O多路复用？

为了使用相同的资源做更多的事，或者做相同的事情使用更少的资源。

#### 如何实现I/O多路复用？

三种实现方式的迭代：`select——>poll——>epoll`，注意从内核态到用户态的拷贝是同步操作会导致阻塞。



###### select/poll

![](./imgs/select.gif)

> select和poll原理一样，poll使用链表突破了select中最大监听1024个文件描述符的限制

###### epoll

![](./imgs/epoll.gif)



[你管这破玩意叫 IO 多路复用？](https://mp.weixin.qq.com/s/JHqVY02mMJIpuZ4s9XOrVg?vid=1688855298418017&deviceid=f0774475-543e-4258-85a3-7234d867a804&version=4.1.0.6011&platform=win)

[谈谈你对IO多路复用机制的理解](https://www.51cto.com/article/717096.html)