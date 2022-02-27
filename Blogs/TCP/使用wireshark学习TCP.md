## TCP标志位:

在TCP传输中，标志位用于表示特定的连接状态或提供额外信息。每个标志位占用1比特。常用的TCP标志位包含以下几种：

+ SYN

  Synchronous，TCP三次握手建立连接的第一步，主动建立连接的一方发送SYN到被动建立连接一方。在下图中可以看到，发送方的SYN被置为1

  ![](imgs\syn_flag.jpg)

+ ACK
  Acknowledgement，用于表示成功收到一个包
  
+ PUSH
  Push，告诉接收方处理接受到的数据包而不是缓冲它们
  
+ FIN
  Finish，表示发送方将不再发送数据，通常用于表示发送方发送的最后一个包
  
+ RST
  Reset，当数据包被发送到预期之外的特定主机时，从接收方发送到发送方
  
+ URG
  Urgent，该标志为用于通知接收方优先处理当前包
  

除此之外还有**ECE**、**ECE**、**NS**等。

## Sequence number && Acknowledgment number

建立连接时，发送方的Seq值是随机的，wireshark工具默认会使用相对值，可以在编辑->首选项->协议一栏选择TCP，进行开启/关闭相对seq值：

![](imgs\relative_seq.jpg)

Seq的值等于发送方的Ack，对于Ack的值，分以下三种情况：

+ 三次握手建立连接期间，Ack的值是发送方的Seq值+1；

+ 连接建立后，Ack的值等于发送方的Len值加上Seq值；
  ![](imgs\seq_value_01.jpg)

+ 断开连接时，Ack的值等于发送方的Seq值+1；
  ![](imgs\seq_value_02.jpg)

## 四次握手断开连接

![](imgs\four_way_handshake.jpg)

以上，图片来自[跟着动画来学习TCP三次握手和四次挥手](https://juejin.im/post/5b29d2c4e51d4558b80b1d8c)。

但使用wireshark捕获到的断开连接过程和上面略有差异，只有三次通讯，将被动关闭一方的两次请求合并为一次：

![](imgs\close_con_with_three_handshake.jpg)

#### 2MSL

TCP协议规定，从主动断开连接一方进入TIME_WAIT状态到真正关闭TCP连接释放Socket资源，最大需要等待2MSL(Max Segment Lifetime)，即4分钟。这样可以确保新的连接不会收到上个连接遗留的数据包。但对于不同的实现，这个时长略有差异。

## TCP/IP模型
TCP位于传输层，提供字节流服务（Byte Streaam Service），即将大块数据分割为以报文段（segment）为单位的数据包进行管理。在网络通讯中，**数据包（package）是最小的传输单位**，网络层负责处理数据包。
![](imgs\tcp_ip.jpg)
以上，图片来自[报文段、数据报、数据包和帧](https://www.cnblogs.com/raykuan/p/6555479.html)一文，

## 小结

因为想要看Redis客户端与服务器通讯的细节，而Redis通讯协议基于TCP，所以就有了这篇笔记。本文对于TCP做了简要介绍，能够满足了解Redis通讯的需要。至于更多的TCP知识，若以后需要用到再行补充。

## 推荐阅读

[跟着动画来学习TCP三次握手和四次挥手](https://juejin.im/post/5b29d2c4e51d4558b80b1d8c)

[理解TCP序列号（Sequence Number）和确认号（Acknowledgment Number）](https://blog.csdn.net/a19881029/article/details/38091243)

[TCP的三次握手、四次挥手--非常详细讲解](https://blog.csdn.net/smileiam/article/details/78226816)

[TCP Flags](https://www.keycdn.com/support/tcp-flags)

[报文段、数据报、数据包和帧](https://www.cnblogs.com/raykuan/p/6555479.html)