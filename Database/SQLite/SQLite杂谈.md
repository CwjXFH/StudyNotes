

SQLite使用[两种B-trees的变体](https://www.sqlite.org/fileformat2.html#b_tree_pages)来存储数据和索引：

+ Table b-trees

  使用64位有符号整数为key，数据都存储在叶子节点上

+ Index b-trees

  使用任意类型为key，不存储数据。索引和without rowid表使用

SQLite性能优化：

+ 过滤操作放到SQLite中而不是代码中

+ 使用WAL模式

+ 减少数据绑定消耗

  SQLite本身很快，但上层库读取数据产生的绑定消耗（CPU/内存）可能比较大





## 推荐阅读

[Query Planning:SQLite索引工作原理](https://www.sqlite.org/queryplanner.html)

二分查找

[Best practices for SQLite performance](https://developer.android.com/topic/performance/sqlite-performance-best-practices#consider-integer)

[SQLite Optimizations for Ultra High-Performance](https://www.powersync.com/blog/sqlite-optimizations-for-ultra-high-performance)