## **WebSocket 与HTTP、 TCP、Socket的关系**

WebSocket 的连接建立过程（握手）是通过 HTTP 协议完成的。客户端先发起一个特殊的 HTTP 请求（带有 `Upgrade: websocket` 头），服务器同意后，连接升级为 WebSocket，后续数据传输就不再使用 HTTP 协议，而是直接在 TCP 通道上传输 WebSocket。

TCP只负责字节流的可靠传输，不关心数据内容和格式。开发者需要自己定义数据的结构和协议。WebSocket 在 TCP 之上，定义了消息帧格式、心跳机制、数据分片等，适合 Web 实时通信场景。它以“消息”为单位进行传输，可以直接传递文本或二进制数据。

Socket并不是协议，而是一组编程接口（API），用于简化 TCP/UDP 等底层协议的使用。如：开发者通过 Socket 编程可以直接操作 TCP 连接，实现自定义的通信协议。



| 协议/技术 | 所属层次 | 连接方式      | 通信模式     | 典型应用场景     |
| --------- | -------- | ------------- | ------------ | ---------------- |
| TCP       | 传输层   | 长连接        | 字节流       | 各类网络通信基础 |
| HTTP      | 应用层   | 短连接/长连接 | 单向请求响应 | 网页、API        |
| WebSocket | 应用层   | 长连接        | 全双工       | 聊天、推送、游戏 |
| Socket    | 编程接口 | 依赖TCP/UDP   | 自定义       | 底层网络开发     |



## SubProtocol

MQTT over WebSocket



## WebSocket over HTTP/2