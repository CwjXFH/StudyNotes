[MySQL :: MySQL 8.0 Reference Manual :: 5.4 MySQL Server Logs](https://dev.mysql.com/doc/refman/8.0/en/server-logs.html)



## binlog

逻辑日志

数据通过追加方式写入日志文件

主从复制

数据恢复



## Slow Query Log



## [InnoDB](https://dev.mysql.com/doc/refman/8.0/en/innodb-architecture.html)

#### Redo Log

物理日志

文件大小固定，循环写入

数据库崩溃恢复，crash-safe

> MySQL两阶段提交

#### Undo Logs

逻辑日志

事务回滚

MVCC



## 推荐阅读

[MySQL的redo log和binlog日志](https://mp.weixin.qq.com/s/bgUs2y8CNhQDnTqyiUalGw)