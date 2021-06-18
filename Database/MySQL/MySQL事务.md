> 基于MySQL 8.0.19，使用`SELECT VERSION();`查看MySQL版本

#### 查询隔离级别

查询当前会话事务隔离级别：

`SELECT @@transaction_ISOLATION;` 或 `SELECT @@session.transaction_ISOLATION;`

查询MySQL服务事务隔离级别：

`SELECT @@global.transaction_ISOLATION;`



#### 设置隔离级别

设置下一个事务(next transaction)隔离级别：

`SET TRANSACTION ISOLATION LEVEL READ COMMITTED;`

设置当前会话隔离级别：

`SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED;`

设置MySQL服务事务隔离级别：

`SET GLOBAL TRANSACTION ISOLATION LEVEL READ COMMITTED;`，服务重启后该设置将失效，可以修改配置文件来避免重启服务配置失效。



[Mysql - How to find out isolation level for the transactions](https://stackoverflow.com/questions/41825832/mysql-how-to-find-out-isolation-level-for-the-transactions)  

[SET TRANSACTION Statement](https://dev.mysql.com/doc/refman/8.0/en/set-transaction.html)



