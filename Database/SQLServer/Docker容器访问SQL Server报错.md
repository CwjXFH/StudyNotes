## SSL Provider, error: 31 - Encryption(ssl/tls) handshake failed

在测试环境部署服务后，调用API会抛出以下异常：

```tex
Microsoft.Data.SqlClient.SqlException (0x80131904): A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: SSL Provider, error: 31 - Encryption(ssl/tls) handshake failed)
 ---> System.IO.IOException:  Received an unexpected EOF or 0 bytes from the transport stream.
```

原因如下：

Docker容器中支持的TLS最低版本为1.2，但对应的SQL Server不支持1.2版本，可通过挂载配置文件的方式将容器支持的TLS最低版本设置为1.0来解决该问题。

**启动容器，然后进入容器内`/etc/ssl`目录下拷贝出`openssl.cnf`文件**，修改TLS配置。尽量不使用其他已存在的`openssl.cnf`文件，可能不兼容导致修改无效：

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



除了通过挂载文件之外，还可以在`Dockerfile`中进行修改：

`Dockerfile`中添加以下两条命令：

```dockerfile
RUN sed -i 's/TLSv1.2/TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
```

一个完整的`Dockerfile`示例：

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish ./src/APIs/APIs.csproj -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN sed -i 's/TLSv1.2/TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "APIs.dll"]
```



### .NET 8

.NET 8连接部署在Windows Server 2012 R2 上的SQL Server 2016 SP1时，出现上述错误，须在openssl.cnf配置文件中添加如下节点：

```shell
[openssl_init]
ssl_conf = ssl_configuration
[ssl_configuration]
system_default = tls_system_default
[tls_system_default]
MinProtocol = TLSv1
CipherString = DEFAULT@SECLEVEL=0
```

可在打包镜像时使用如下命令添加：

```shell
RUN sed -i 's|\[openssl_init\]|&\nssl_conf = ssl_configuration\n[ssl_configuration]\nsystem_default = tls_system_default\n[tls_system_default]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0|' /etc/ssl/openssl.cnf
```



但，同一套代码，连接另外两台数据库不出错，配置分别如下：

```
SOL Server 2016 SP1
Windows Server 2016
```

```
SQL Server 2016 SP3
Windows Server 2022
```

具体细节待研究



## Connection Timeout Expired

容器中连接数据库报超时错误：
```tex
An exception occurred while iterating over the results of a query for context type'SqlServerRepository.DataBackgroundDbContext'.
Microsoft.Data.SqlClient.SqlException (0x80131904): Connection Timeout Expired.  The timeout period elapsed during the post-login phase.  The connection could have timed out while waiting for server to complete the login process and respond; Or it could have timed out while attempting to create multiple active connections.  The duration spent while attempting to connect to this server was - [Pre-Login] initialization=45; handshake=334; [Login] initialization=5; authentication=22; [Post-Login] complete=14299
```

通过`ping`以及`telnet`命令确认容器到数据库的网络是通顺的，具体原因如下：

数据库版本是SQL Server 2008，只打了SP1的补丁，在linux环境下SqlClient库无法连接到数据库，升级安装SP3后问题解决。



Github上[SqlClient](https://github.com/dotnet/SqlClient)项目Issues下挺多关于数据库连接相关问题。

## 推荐阅读

[Login-phase errors](https://docs.microsoft.com/en-us/sql/connect/ado-net/sqlclient-troubleshooting-guide?view=sql-server-ver16#login-phase-errors)  

[KB3135244 - TLS 1.2 support for Microsoft SQL Server](https://support.microsoft.com/en-us/topic/kb3135244-tls-1-2-support-for-microsoft-sql-server-e4472ef8-90a9-13c1-e4d8-44aad198cdbe)  

[Multi-stage builds](https://docs.docker.com/build/building/multi-stage/)

