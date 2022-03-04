SQL Server中的锁分为两类：
+ 共享锁
+ 排它锁

锁的兼容性：事务间锁的相互影响称为锁的兼容性。

| 锁模式       | 是否可以持有排它锁 | 是否可以持有共享锁 |
| ------------ | ------------------ | ------------------ |
| 已持有排它锁 | 否                 | 否                 |
| 已持有共享锁 | 否                 | 是                 |

SQL Server中可以锁定的资源包括：RID或键（行）、页、对象（如表）、数据库等等。

**在试图修改数据（增删改）时，事务会请求数据资源的一个排它锁而不考虑事务的隔离级别**。排它锁直到事务结束才会解除。对于单语句事务，语句执行完毕该事物就结束了；对于多语句事务，执行完COMMIT TRAN或者ROLLBACK TRAN命令才意味着事务的结束。

**在事务持有排它锁期间，其它事务不能修改该事物正在操作的数据行，但能否读取这些行，则取决于事务的隔离级别。**

在试图读取数据时，事务默认请求数据资源的共享锁，事务结束时会释放锁。**可以通过事务隔离级别控制事务读取数据时锁定的处理方式**。

---

SQL Server中事务隔离级别分为以下两大类：

+ 基于悲观并发控制（会话级别）的四个隔离级别（隔离级别自上而下依此增强）：
  - READ UNCOMMITTED
  - READ COMMITTED（默认）
  - REPEATABLE READ
  - SERIALIZABLE 

不同会话之间的隔离级别及会话嵌套间的隔离级别互不影响，详情可参阅[此处](https://docs.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql?view=sql-server-2017#remarks)。

+ 基于乐观并发控制（数据库级别）的两个隔离级别（隔离级别自上而下依此增强）：
  - SNAPSHOT
   - READ COMMITTED SNAPSHOT（默认）

可以通过下面的语句来**设置会话的隔离级别**：
```SQL
SET TRANSACTION ISOLATION LEVEL <isolation name>
```

隔离级别可以确定并发用户读取或写入的行为。在获得锁和锁的持续期间，不能控制写入者的行为方式，当时可以控制读取者的行为方式。此外，也可通过控制读取者的行为方式来隐式的影响写入者的行为。隔离级别越高读取者请求的锁越强，持续时间越长，数据一致性越高，并发性越低。

`READ COMMITTED SNAPSHOT`和`SNAPSHOT`可以看作是`READ COMMITTED`和`SERIALIZABLE`对应的乐观并发控制实现。


在事务持有一个数据资源的锁时，若另一个事务请求该资源的不兼容锁时，请求会被阻塞而进入等待状态。该请求一直等待直至被锁定的资源释放或者等待超时。可以通过语句以下语句来查询数据库中事务锁信息：
```SQL
--获取当前会话Id
SELECT @@SPID;
--查询数据库中锁信息
SELECT * FROM sys.dm_tran_locks;
--使用KILL命令关闭id为52的会话
--注意KILL命令不是SQL而是SQL Server用于管理数据库的命令
--KILL命令会回滚事务
KILL 52;
```

设置锁超时时间，锁超时不会回滚事务：
```SQL
--设置锁超时时间为5S
SET LOCK_TIMEOUT 5000;
--取消超时时间限制
SET LOCK_TIMEOUT -1;
```

### READ UNCOMMITTED

在该隔离级别中，读取者无需请求共享锁，从而也不会与持有排它锁的写入者发生冲突。如此，读取者可以读到写入者尚未提交的更改。即，**脏读**。
在查询语句中`READ COMMITTED`可以简写为`NOLOCK`：
```SQL
SELECT * FROM A WITH(NOLOCK)
```

### READ COMMITTED

在该隔离级别中，读取者必须获取一个共享锁以防止读取到未提交的数据。这意味着，若有其它事务正在修改资源则读取者必须进行等待，当写入者提交事务后，读取者就可以获得共享锁进行读取。

该隔离级别中，事务所持有的共享锁不会持续到事务结束，当查询语句结束（甚至未结束）时，便释放锁。这意味着在同一个事物中，两次相同数据资源的读取之间，不会持有该资源的锁，因此，其它事务可以在两次读取间隙修改资源从而导致两次读取结果不一致，即**不可重复读**，同时该隔离级别下也会产生**更新丢失**问题。

### REPEATABLE READ

在该隔离级别中，读取者必须获取共享锁且持续到事务结束。该隔离级别获得的共享锁只会**锁定执行查询语句时符合查询条件的资源**。举例如下：
```SQL
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
BEGIN TRAN
SELECT * FROM A WHERE Id<10;
```
上述语句只会锁定符合Id<10条件的数据行，若表中Id<10的数据有Id=2,3,4,5,6五条，那么只会锁定这五条数据：
```SQL
--阻塞
DELETE FROM A WHERE Id=2;
--不会阻塞
DELETE FROM A WHERE Id=7;
--阻塞
UPDATE A SET Name='' WHERE Id=2;
--不会阻塞
UPDATE A SET Name='' WHERE Id=7;
--不会阻塞，且新插入的数据不会被锁定，可以执行更新和删除操作
--这就会导致幻读问题，可参考MySQL间隙锁（GAP）
INSERT INTO A(Id,Name) VALUES(7,'5');
```
该隔离级别下可以避免更新丢失问题，但会产生**幻读**，即同一事务两次相同条件的查询之间插入了新数据，导致第二次查询获取到了新的数据。

### SERIALIZABLE

在该隔离级别中，读取者必须获取共享锁且持续到事务结束。该隔离级别的共享锁不仅锁定执行查询语句时符合查询条件的数据行，也会锁定将来可能用到的数据行。即，**阻止可能对当前读取结果产生影响的所有操作**。

举例如下：
```SQL
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
BEGIN TRAN
SELECT * FROM A WHERE Id<10;
```
上述语句只会锁定符合Id<10条件的数据行，若表中Id<10的数据有Id=2,3,4,5,6五条，则：
```SQL
--阻塞
DELETE FROM A WHERE Id=2;
--不会阻塞
DELETE FROM A WHERE Id=7;
--阻塞
UPDATE A SET Name='' WHERE Id=2;
--不会阻塞
UPDATE A SET Name='' WHERE Id=7;
--阻塞，这里与 REPEATABLE READ 不一样
INSERT INTO A(Id,Name) VALUES(7,'5');
```

---
`SNAPSHOT`和`REAED COMMITTED SNPSHOT`是SQL Server基于行版本控制技术的隔离级别，在这两个隔离级别中，**读取者不会获取共享锁**。SQL Server可以在tempdb库中存储已提交行的之前版本。如果当前版本不是读取者所希望的版本，那么SQL Server会提供一个较旧的版本。

`SNAPSHOT`在逻辑上与`SERIALIZABLE`类似；`READ COMMITTED SNPSHOT`在逻辑上与`READ COMMITTED`类似。这两个隔离级别中执行`DELETE`和`UPDATE`语句需要复制行的版本，`INSERT`语句则不需要。因此，对于更新和删除操作的性能会有负面影响，因无需获取共享锁，所以读取者的性能通常会有所改善。

### SNAPSHOT

在该隔离级别中，读取者在读取数据时，它是确保获得**事务启动时**最近提交的可用行版本。这意味着，保证获得的是提交后的读取并且可以重复读取，以及确保获得的不是幻读，就像是在`SERIALIZABLE`级别中一样。但该隔离级别并不会获取共享锁。

启用该隔离级别需要先执行下面的语句：
```SQL
--需要在数据库级别启用基于快照的隔离级别
ALTER DATABASE DbName SET ALLOW_SNAPSHOT_ISOLATION ON;  
```
```SQL
--修改数据不提交事务
BEGIN TRAN
  UPDATE A SET Name='22' WHERE Id=2;
```
```SQL
SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
--查询不会被阻塞
--上述事务提交之前返回可用的旧版本，提交后则返回修改后的结果
SELECT * FROM xfh.[Table] WHERE Id=2;
```
##### 冲突检测

该隔离级别的事务中，SQL Server会进行冲突检测以防止更新冲突，这里的检测不会引起死锁问题。即，若该隔离级别的事务在修改数据时，若发现已有其它事务修改了相同版本号的数据，则会引发下面的错误：

```
消息 3960，级别 16，状态 2，第 4 行
快照隔离事务由于更新冲突而中止。您无法在数据库'Test'中使用快照隔离来直接或间接访问表 'A'，
以便更新、删除或插入已由其他事务修改或删除的行。请重试该事务或更改 update/delete 语句的隔离级别。
```

### READ COMMITTED SNPSHOT

该隔离级别与`SNAPSHOT`的不同之处在于，读取者获得是**语句启动时**（不是事务启动时）可用的最后提交的行版本。

启用该隔离级别需要先执行下面的语句：
```SQL
--需要在数据库级别启用基于快照的隔离级别
--要保证执行该语句的链接必须是目标数据库的唯一链接
ALTER DATABASE Test SET READ_COMMITTED_SNAPSHOT ON;
```
---

| 隔离级别                | 允许脏读？ | 允许不可重复读？ | 允许丢失更新？ | 允许幻读？ | 检测更新冲突？ | 使用行版本控制？ |
| ----------------------- | ---------- | ---------------- | -------------- | ---------- | -------------- | ---------------- |
| READ UNCOMMITTED        | 是         | 是               | 是             | 是         | 否             | 否               |
| READ COMMITTED          | 否         | 是               | 是             | 是         | 否             | 否               |
| REPEATABLE READ         | 否         | 否               | 否             | 是         | 否             | 否               |
| SERIALIZABLE            | 否         | 否               | 否             | 否         | 否             | 否               |
| SNAPSHOT                | 否         | 否               | 否             | 否         | 是             | 是               |
| READ COMMITTED SNAPSHOT | 否         | 是               | 是             | 是         | 否             | 是               |

### 死锁
对于死锁，SQL Server会自行清理。默认情况下，SQL Server会选择终止工作量少的事务以解除死锁，因为工作量少便于事务的回滚操作。用户也可以设置死锁优先级`DEADLOCK_PRIORITY`，这样优先级低的便被终止，而不管其工作量大小。

### 结语
SQL Server中提供了四种不依赖行版本控制的事务隔离级别，及两种依赖行版本控制的事务隔离级别。**不同事务的隔离级别会对数据查询语句的执行过程（是否获取共享锁，语句是否会被阻塞）及结果（是否有脏读、幻读等）产生较大的影响，对于修改数据行为的影响仅限于是否会阻塞语句的执行，因为修改数据的语句必须要获取排它锁才能被执行**。

以上是自己《SQL Server2012 T-SQL基础教程》事务与并发处理一章的读书笔记，错误之处望各位多多指教。

### 推荐阅读
[数据库村的旺财和小强 ](https://mp.weixin.qq.com/s/bM_g6Z0K93DNFycvfJIbwQ)  
[sql server锁知识及锁应用](https://blog.csdn.net/huwei2003/article/details/4047191)  
[数据库两大神器【索引和锁】](https://mp.weixin.qq.com/s?__biz=MzI4Njg5MDA5NA==&mid=2247484292&idx=1&sn=27c9ae4945b76540ca9e5aad88576729&chksm=ebd74285dca0cb9352ad63e25de0e7657909e2a39dab7da43b3aca13d38a12a757ab5a9f3e13#rd)  
[SET TRANSACTION ISOLATION LEVEL (Transact-SQL)](https://docs.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql?view=sql-server-2017)

### 书目推荐

《SQL Server2012 T-SQL基础教程》