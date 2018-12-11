## 子查询

在嵌套查询中，最外面查询结果集返回给调用方，称为外部查询。嵌套在外部查询内的查询称为子查询，**子查询的结果集供外部查询使用**。

根据是否依赖外部查询，可将子查询分为自包含子查询和相关子查询。自包含子查询不依赖外部查询，相关子查询则依赖外部查询。

子查询结果是在运行时计算的，查询结果会跟随查询表的变化而改变。子查询可以返回单个值（标量）、多个值或者整个表结果。

**在逻辑上，子查询代码仅在外部查询计算之前计算一次**。

#### 自包含子查询

```mssql
USE WJChi;

SELECT * FROM dbo.UserInfo WHERE Age=
(
	SELECT MAX(Age) FROM dbo.UserInfo	
);
```

#### 相关子查询

```mssql
USE WJChi;

SELECT * FROM dbo.UserInfo AS UI WHERE IdentifyId =
(
	SELECT Id FROM dbo.Identify WHERE Id=UI.IdentifyId
);
```

#### 子查询易错点

###### NULL值处理不当

```mssql
USE WJChi;

SELECT * FROM dbo.Customers
WHERE custid NOT IN(
	SELECT TOP 10 C.custid FROM dbo.Customers AS C ORDER BY C.custid
);
```

上述查询语句看起来可以正常运行，但当子查询的返回结果集中包含NULL值时，上述查询语句则不会返回任何数据。解释如下：

`20 NOT IN(10, 9, 8, NULL)`等价于`NOT(20=10 OR 20=9 OR 20=8 OR 20=NULL)`，`NULL`参与的比较预算结果均为`Unknown`，`Unknown`参与的或运算结果依然为`Unknown`。

> :warning: 我们应时刻牢记SQL是三值逻辑，这点很容易引发错误

###### 列名处理不当

子查询中的列名首先从当前查询中进行解析，若未找到则到外部查询中查找。子查询中很有可能无意中包含了外部查询的列名导致子查询有自包含子查询变为相关子查询而引发逻辑错误。

为避免上述错误，查询中的列名尽可能使用完全限定名：`[表名].[列名]`。

> :warning: 通常我们自己难以发现代码中的逻辑错误，而我们的最终用户尝尝扮演着问题发现者的角色 :joy:
>
> 编写语义清晰明了的SQL可以很大程度的避免逻辑上的错误

## 表表达式

表表达式，也可称为**表子查询**，是一个命名的查询表达式，表示一个有效的关系表，因此表表达式必须满足以下三个条件：

1. 无法保证表表达式结果集顺序

表表达式表示一个关系表，关系型数据库基于集合理论，表中的数据是无序的。**标准SQL中不允许在表表达式中使用`ORDER BY`子句，除非`ORDER BY`子句用于展示之外的其他目的**，否则会报错：

<font color="red" size=3>除非另外还指定了 TOP、OFFSET 或 FOR XML，否则，ORDER BY 子句在视图、内联函数、派生表、子查询和公用表表达式中无效.</font>

>  :warning:在查询表表达式时，除非在外部查询中指定了`ORDER BY`子句，否则无法保证查询结果集中数据的顺序。有时候会看到即使外部查询未使用`ORDER BY`但查询结果集按预期顺序返回了结果，这是由于数据库自身优化的结果，依然无法保证每次查询都能按预期结果返回。

2. 所有列必须显式指定名称

3. 所有列名必须唯一

表表达式分为：派生表、公用表表达式、视图三种类型。其中，**派生表与公用表表达式只适用于单语句范围，即，只存在于当前查询语句中。视图则可以被多条查询语句复用**。

#### 派生表

派生表又称为子查询表，**在外部查询的FROM子句中进行定义**，一旦外部查询结束，派生表也就不复存在。

在一次查询中派生表无法被多次引用，若要多次引用，则需要多次书写派生表：

```mssql
USE WJChi;

SELECT 
	Cur.orderyear, Prv.numcusts AS prvnumcusts, 
	Cur.numcusts - Prv.numcusts AS growth
FROM (
	SELECT 
    	YEAR(orderdate) AS orderyear, COUNT(DISTINCT custid) AS numcusts
    FROM dbo.Orders 
    GROUP BY YEAR(orderdate) AS Cur
    LEFT JOIN
    -- 为了再次使用派生表，需要重复书写相同逻辑
    SELECT 
    	YEAR(orderdate) AS orderyear, COUNT(DISTINCT custid) AS numcusts
    FROM dbo.Orders 
    GROUP BY YEAR(orderdate) AS Prv
    ON Cur.orderyear = Prv.orderyear + 1
);
```

#### 公用表表达式

公用表表达式(CTE)定义方式如下：

```mssql
WITH...AS
(
	...
)
```

与派生表类似，外部查询完成后，CTE也就消失了。但，不同于派生表，CTE可以在一次查询中多次使用（但不能嵌套使用而派生表可以）：

```mssql
USE WJChi;

WITH YearlyCount AS
(
	SELECT 
    	YEAR(orderdate) AS orderyear, COUNT(DISTINCT custid) AS numcusts
    FROM dbo.Orders
    GROUP BY YEAR(orderdate)
)
SELECT 
	Cur.orderyear, Prv.numcusts AS prvnumcusts
FROM YearlyCount AS Cur
LEFT JOIN
-- 再次使用CTE
YearlyCount AS Prv
ON Cur.orderyear = Prv.orderyear + 1;
```

这里需要注意一点：CTE之前的SQL语句要以分号（;）结尾。

我们也可以在一次查询中定义多个CTE：

```mssql
-- WITH只需要使用一次
WITH Temp1 AS
(
),
Temp2 AS
(
)
SELECT ...
```


#### 视图

视图是虚拟表，自身不包含数据，只存储了动态查询语句，多用于简化复杂查询。

视图创建后被作为数据库对象而存储到数据库中，除非显式进行删除。因此，同一个视图可以被不同的查询多次使用。

使用以下语句创建视图：

```mssql
CREATE VIEW ViewName
AS
...
```

修改视图：

```mssql
ALTER VIEW ViewName
AS
...
```

删除视图：

```mssql
DROP VIEW ViewName;
```

视图是数据库中的对象，因此我们可以控制其访问权限，如：SELECT、UPDATE或访问视图底层数据表等。

视图一旦创建，在底层数据表发生变更后，其不会自动更新。因此，在视图中使用SELECT语句时尽可能显式的指定所需列，而不是使用`SELECT *`。可以使用存储过程：`sp_refreshview`和`sp_refreshsqlmodule`来更新视图的元数据，或者使用ALTER语句修改视图定义。

关于是否应该使用视图，仁者见仁，智者见智：

[使用SQL Server视图的优缺点](http://database.51cto.com/art/201011/233240.htm)

[为什么mysql中很少见到使用视图功能？](https://www.zhihu.com/question/26614127)

## 小结

不要让数据库（查询）变得复杂；

表表达式有助于简化代码以提升可读性与可维护性；

## 推荐阅读

[T-SQL基础（二）之关联查询](https://www.cnblogs.com/Cwj-XFH/p/9960822.html)