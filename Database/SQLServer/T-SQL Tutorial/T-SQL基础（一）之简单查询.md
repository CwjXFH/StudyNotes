## 名词解释
**SQL:** Structured Query Language，结构化查询语言，是一种在关系型数据库中用于管理数据的**标准语言**。SQL是一种声明式编程语言，即只需表明需要什么而无需关注实现细节（C#中的LINQ也是如此）。

**SQL方言：**在SQL标准的基础上延伸的其它语言，如SQL Server中所使用的T-SQL。注意SQL方言未必完全支持所有的SQL标准。

**T-SQL:**Transact-SQL，微软公司提供的用于SQL Server数据库的SQL方言（扩展）。

## SQL表达式运算

#### 谓词

SQL中谓词是指运算结果为True,False或Unknown的逻辑表达式。T-SQL中的谓词有IN，BETWEEN，LIKE等。

使用LIKE可以做模糊匹配，支持正则表达式：

```sql
-- 查找以x开头的name
SELECT name FROM WJChi.dbo.UserInfo WHERE name LIKE 'x%';

-- 查找以两个x开头的name，_表示匹配单个字符
SELECT name FROM WJChi.dbo.UserInfo WHERE name LIKE '_x%';
```

需要注意的是，LIKE模糊匹配若以%开头，则无法使用索引。如：`LIKE '%x'`

#### 运算符

SQL中的运算符与高级编程语言（C#,JAVA）类似。当多个运算符出现在同一表达式中时，SQL Server会按照运算符的优先级进行计算。当搞不清楚优先级就使用括号，对比下面两句SQL：

```sql
SELECT orderid FROM Sales.dbo.Orderes 
WHERE 
	custid=1 AND empid IN (1,2,3) 
	OR
	custid=85 AND empid IN  (4,5,6)


SELECT orderid FROM Sales.dbo.Orderes 
WHERE 
	(custid=1 AND empid IN (1,2,3)) 
	OR 
	(custid=85 AND empid IN  (4,5,6))
```

AND运算符的优先级高于OR，所以上述两句SQL在逻辑上等价。但，很明显第二句的WHERE条件逻辑上更清晰。

#### 三值逻辑


SQL中表达式的运算结果有三种情况：`True,False 与 Unknown`。

在查询筛选中，只返回条件表达式（WHERE、HAVING、ON）运算结果为True的数据。

CHECK约束，返回表达式运算结果不为False的结果。

#### 两值逻辑

与T-SQL中的大多数谓词不同，EXISTS使用两值逻辑（True/False），而不是三值逻辑；

在`EXISTS( SELECT * FROM T_A WHERE Id=12)`中，EXISTS谓语只关心匹配行是否存在，而不管SELECT中指定的属性，**就像整个SELECT子句是多余的一样**。SQL Server引擎在优化查询时会忽略SELECT子句。所以，SELECT子句中的星号（\*）对于性能没有任何负面影响。

为节省微不足道的与星号（\*）解析相关的额外成本，而牺牲代码可读性是不值得的。

#### NULL & Unknown

**NULL表示值是Unknown状态**，SQL中不同的语言元素对于NULL有着不同的处理方式。

在使用NULL值时应注意以下几点：

+ 将NULL与其它值进行比较，不管该值是否为NULL，结果均为Unknown

+ 应使用IS NULL或IS NOT NULL来判断值是否为NULL

+ INSERT未给列指定值则插入NULL

+ GROUP BY和ORDER BY子句会将多个NULL值视为相等

+ 标准SQL的UNIQUE约束认为NULL是为彼此不同

+ T-SQL中的UNIQUE约束认为多个NULL是相等的

+ COUNT(*)的特殊性

  若列名为tag的例中存在`a,NULL,c,d`几行数据，那么COUNT(*)返回4而COUNT(tag)则返回3


NULL参与的逻辑运算结果很可能是Unknown（三值逻辑也是引发应用错误的重要原因），除非运算结果不依赖于Unknown，示例如下。

Unknown参与AND运算结果：


|Expression 1|Expression 2|Result|
|---|---|---|
|TRUE|UNKNOWN|UNKNOWN|
|UNKNOWN|UNKNOWN|UNKNOWN|
|FALSE|UNKNOWN|FALSE|

Unknown参与OR预算结果：


|Expression 1|Expression 2|Result|
|---------------|---------------|------------|
|TRUE|UNKNOWN|TRUE|
|UNKNOWN|UNKNOWN|UNKNOWN|
|FALSE|UNKNOWN|UNKNOWN|

## 查询

SQL中的查询是指，SELECT语句经过一些列逻辑处理而获取数据的过程。

几条建议：

- SQL中的关键字均使用大写字母

- SQL语句均使用分号结尾

- SQL中使用对象的完全限定名，如：DbName.dbo.TableName


#### 查询语句执行顺序

SQL中查询语句的逻辑处理过程与实际查询过程（物理查询过程）是有差异的，即，SELECT语句的执行顺序与书写顺序是有差异的。按照SELECT语法规定书写的SQL语句较为符合英语语法习惯（对人类友好），但SELECT语句的实际执行则按照如下顺序进行（对机器友好）：

- FROM
- JOIN ON
- WHERE
- GROUP BY
- HAVING
- SELECT
- 表达式
- DISTINCT
- ORDER BY
- TOP/OFFSET FETCH

 OFFSET FETCH可以看作是ORDER BY子句的一部分

> :warning: SQL基于集合理论，查询结果集（表结果）是无顺寻的（虽然看起来结果集像按照某种顺序排列），除非显式的使用ORDER BY子句指定顺寻，但使用ORDER BY字句后结果集将被作为游标对待，而非表结果。

FROM子句用于指定需要查询的数据源，WHERE语句对数据源中的数据做基于行的筛选。通常WHERE子句可以决定查询是否使用索引，及使用哪些索引，对于查询优化有着重要意义。

GROUP BY子句用于对查询结果集进行分组，GROUP BY之后的所有操作都是对组而非行的操作。在查询结果中，每组最终由一个单行来表示。这意味着，GROUP BY之后的所有子句中指定的表达式必须对每组返回一个标量（单个值）。

HAVING用于对GROUP BY产生的**组进行筛选**。

SELECT语句用于指定返回到查询结果集中的列，生成查询结果表。注意，在SELECT子句之前执行的子句无法使用SELECT子句中的**列的别名**，否则会返回`Invalid column name`错误。

TOP不是标准SQL，是T-SQL专有功能，用于限制查询返回的指定行数或百分比：

```sql
-- 返回Table中的10条数据
SELECT TOP(10) * FROM Table;

-- 返回Table中10%的数据
SELECT TOP(10) PERCENT * FROM Table;
```

OFFSET-FETCH有着与TOP类似的功能，但它是标准SQL，可用于分页查询：

```sql
-- 取第51至60行的10条数据
SELECT * FROM Table
ORDER BY Id DESC
OFFSET 50 ROWS FETCH NEXT 10 ROWS ONLY;
```

注意SQL SERVER中，OFFSET-FETCH要与ORDER BY结合使用，否则会报错：

<font color=red size=3>Invalid usage of the option NEXT in the FETCH statement.</font>

#### 同时操作

SQL中有all-at-once operations（同时操作）的概念，即出现在同一逻辑处理阶段的所有表达式在同一时间进行逻辑计算。

因为同时操作的原因，下面示例中orderyear+1中的oderyear是无效的，SQL会报错：`Invalid column name 'orderyear'`：

```sql
SELECT orderid,YEAR(orderdate) AS orderyear,orderyear+1 AS nextyear FROM Sales.dbo.Orders;
```

同样，由于同时操作的原因，SQL Server不支持短路操作。如，WHERE子句中的多个表达式的计算并没有确定的顺序。

#### CASE...WHEN...

CASE表达式是标量表达式，返回一个符合条件的值。注意，CASE是表达式，不是语句，与COUNT类似。

CASE表达式有两种使用方式：

+ CASE后面带有列名

  这种情况下，`WHEN`子句中只能使用标量或返回标量的表达式，这种形式称为简单格式。

```sql
SELECT 
Name,
CASE Age
	WHEN 50 THEN '知天命'
	WHEN 1+1 THEN ''
	ELSE '未成年'
END
FROM WJChi.dbo.UserInfo;
```

+ CASE后面不带列名

  这种情况下，`WHEN`子句中只能使用逻辑表达式，这种形式称为搜索格式。

```sql
SELECT 
Name,
CASE 
	WHEN Age BETWEEN 60 AND 100 THEN '老年'
	WHEN Age>=18 THEN '成年'
	WHEN Name='雪飞鸿' THEN '666'
	WHEN 1+1=2 THEN ''
	ELSE '你猜'
END
FROM WJChi.dbo.UserInfo;
```

CASE表达式中若未指定ELSE的返回值，则默认为`ELSE NULL`。

#### 查询分类

查询可分为：

+ 单表查询

  查询中最简单的一种形式。高并发，分布式系统中常用。通常单表查询仅需一句SELECT语句即可，简单且数据库

+ 联接查询

  INNER JOIN、LEFT JOIN、RIGHT JOIN、CROSS JOIN

+ 子查询

  SQL可以在一个查询语句中编写另外一个查询语句，即嵌套查询。最外面的查询结果集返回给调用者，称为外部查询。内部查询的结果集被用于外部查询，称为子查询。

+ 表表达式

  派生表、公用表表达式、视图等

#### 聚合函数

聚合函数对多行数据进行运算后返回标量（聚合），只有SELECT、HAVING、ORDER BY语句中可以使用聚合函数；

#### 开窗函数

开窗函数是对基本查询中的每一行按组（窗口）进行运算，并得到一个标量。行的窗口使用OVER子句定义。

#### 锁与事务隔离级别

SQL Server默认情况下，查询语句会申请共享锁。共享锁可以阻止对数据进行修改，详细信息可参阅：[SQL Server中锁与事务隔离级别](https://www.cnblogs.com/Cwj-XFH/p/9313882.html)

## 小结

相较于增删改而言，查询是比较复杂的，也是数据库优化的关注重点。本文主要介绍了T-SQL查询的基础知识，对于较为复杂的查询，如：关联、表表达式、集合运算等将在后续文章中介绍。


## 书籍推荐
《SQL SERVER 2012 T-SQL 基础教程》
《SQL SERVER 性能优化与管理的艺术》
《SQL SERVER基础教程》

## 推荐阅读

[NULL and UNKNOWN (Transact-SQL)](https://docs.microsoft.com/en-us/sql/t-sql/language-elements/null-and-unknown-transact-sql?view=sql-server-2017)

[SQL Server中锁与事务隔离级别](./SQL%20Server中锁与事务隔离级别.md)

[数据库两大神器【索引和锁】](https://cloud.tencent.com/developer/article/1170837)

[SQL SERVER开窗函数](https://www.cnblogs.com/csdbfans/p/3504845.html)