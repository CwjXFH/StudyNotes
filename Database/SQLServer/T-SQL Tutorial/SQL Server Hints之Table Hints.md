最近再将公司中依赖存储过程的业务使用EFCore重写，有个存储过程中使用了UPDLOCK和HOLDLOCK。以前使用比较多的是NOLOCK，偶尔使用下UPDLOCK，对于HOLDLOCK之前没用过，查询下资料，有了这篇博文。

不管是NOLOCK、UPDLOCK还是HOLDLOCK，均属于[SQL Server Table Hints](https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table?view=sql-server-ver15)范畴。先来了解下Hints的概念，Hints直译过来是提示的意思，这里直接使用Hints。

### Hints

Hints是SQL Server查询处理器为CRUD操作（SELECT、INSERT、UPDATE、DELETE）指定的选项或策略。Hints行为会覆盖查询优化器为查询选择的任何执行计划。

通常SQL Server查询优化器会选择最合适的执行计划来执行查询操作，所以非必要不要使用<join_hint>、<query_hint>、和<table_hint>

### Table Hints

Table Hints主要影响DML的行为



### 推荐阅读

[Hints (Transact-SQL)](https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql?view=sql-server-ver15)  

[Transaction locking and row versioning guide](https://docs.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide?view=sql-server-ver15#lock_modes)

[All about locking in SQL Server](https://www.sqlshack.com/locking-sql-server/)

[SQL Server and Azure SQL index architecture and design guide]([SQL Server and Azure SQL index architecture and design guide - SQL Server | Microsoft Docs](https://docs.microsoft.com/en-us/sql/relational-databases/sql-server-index-design-guide?view=sql-server-ver15))

[SQL Server中锁与事务隔离级别](./SQL%20Server中锁与事务隔离级别.md)  

