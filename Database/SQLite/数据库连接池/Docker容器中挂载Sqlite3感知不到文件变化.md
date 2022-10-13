## 问题描述：

[EFCore cannot get new value in docker, if use rsync replace sqlite file](https://github.com/dotnet/efcore/issues/29302)

基于.NET6开发一个查询SQLite的API，使用Docker进行部署，通过挂载的方式来访问数据库文件：`docker run -d --name ddocker -p 9100:80 -v /mnt/c/Users/chiwenjun/Desktop/pdemo/docs/:/home/db/ddocker` 使用**rsync**命令从其他目录同步文件到`/mnt/c/Users/chiwenjun/Desktop/pdemo/docs/`目录下，API依然返回旧数据。

## 原因解释：

查询SQLite数据库时，数据库中符合查询条件的数据会以页缓存（和连接相关）的形式存放到内存中，减少后续查询的磁盘I/O操作；

**rsync**命令会导致文件inode值发生改变，**cp**命令不会；

修改数据库文件会导致页缓存失效；

.NET串默认开启了数据库连接池，使用**rsync**命令同步文件不会使页缓存失效，所以应用查询依然走页缓存，也就无法感知到数据变化，连接字符串中禁用连接池，可解决该问题。

为什么本地可以，因为本地是Windows环境。在Mac上测试呈现出和Linux上一样的问题。

##　参考：

[Can cached database connections be aware of file changes?](https://github.com/TryGhost/node-sqlite3/issues/1277)

[SqliteCacheMode Enum](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecachemode?view=msdata-sqlite-6.0.0)

[SQLite Page cache](https://www2.sqlite.org/fileio.html#tocentry_132)

[What exactly is being cached when opening/querying a SQLite database](https://unix.stackexchange.com/questions/712735/what-exactly-is-being-cached-when-opening-querying-a-sqlite-database)
