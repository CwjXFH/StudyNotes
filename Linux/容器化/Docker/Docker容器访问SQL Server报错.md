在测试环境部署服务后，调用API会抛出以下异常：

```tex
Microsoft.Data.SqlClient.SqlException (0x80131904): A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: SSL Provider, error: 31 - Encryption(ssl/tls) handshake failed)
 ---> System.IO.IOException:  Received an unexpected EOF or 0 bytes from the transport stream.
```

原因如下：

Docker容器中支持的TLS最低版本为1.2，但对应的SQL Server不支持1.2版本，可通过挂载配置文件的方式将容器支持的TLS最低版本设置为1.0来解决该问题。

启动容器，然后进入容器内`/etc/ssl`目录下拷贝出`openssl.cnf`文件，修改TLS配置：

```tex
[system_default_sect]
MinProtocol = TLSv1
CipherString = DEFAULT@SECLEVEL=1
```

将修改后的`openssl.cnf`文件挂载到容器上：

```yaml
/home/services/conf/openssl.cnf:/etc/ssl/openssl.cnf
```

> :warning:上述做法可能存在安全隐患，官方比较推荐的做法是使用支持TLS1.2的SQL Server版本



## 推荐阅读

[Login-phase errors](https://docs.microsoft.com/en-us/sql/connect/ado-net/sqlclient-troubleshooting-guide?view=sql-server-ver16#login-phase-errors)

[KB3135244 - TLS 1.2 support for Microsoft SQL Server](https://support.microsoft.com/en-us/topic/kb3135244-tls-1-2-support-for-microsoft-sql-server-e4472ef8-90a9-13c1-e4d8-44aad198cdbe)

