常用限流算法有[漏桶算法和令牌桶算法](https://www.cnblogs.com/fangh816/p/13305411.html?utm_source=tuicool)，本文借助Redis的[redis_cell](https://github.com/brandur/redis-cell)模块来实现令牌桶算法限流。

### 构建镜像并启动容器

```dockerfile
FROM redis:latest

ARG cell_dir=/lib/redis_modules/redis_cell
RUN mkdir -p ${cell_dir}
WORKDIR ${cell_dir}

RUN apt-get update \
     && apt-get -y install wget \
     && wget https://github.com/brandur/redis-cell/releases/download/v0.3.0/redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && tar -zxvf redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && rm redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && apt-get -y remove wget \
     && apt -y autoremove \
     && ls -alh

ENTRYPOINT ["redis-server","--loadmodule","/lib/redis_modules/redis_cell/libredis_cell.so"]
```

```shell
docker build -t redis_limit .
docker run -d -p 6379:6379 --rm --name redis_limit redis_limit
```



### 模拟有波动的请求

redis_cell模块提供了原子性命令来实现限流，我们只需要根据命令执行结果来做响应处理即可，不用自己再处理令牌发放相关逻辑：

```python
import random
import time
import redis

r: redis.Redis = redis.Redis(host='localhost', port=6379, db=0)

exec_result = r.execute_command('cl.throttle rate_limit 100 50 10 1')
while True:
    if exec_result[0] == 1:
        seconds = exec_result[3]
        if seconds <= 0:
            seconds = 1
        print(f'等待{seconds}秒后重试')
        time.sleep(seconds)
        print('重试')
    token_num = int(random.random() * 30)
    print(f'剩余{exec_result[2]}个令牌，即将获取{token_num}个令牌')
    if token_num <= 0:
        token_num = 1
    exec_result = r.execute_command(f'cl.throttle rate_limit 100 50 10 {token_num}')

r.close()

# 输出如下：
# 剩余81个令牌，即将获取3个令牌
# 剩余78个令牌，即将获取28个令牌
# 剩余50个令牌，即将获取24个令牌
# 剩余26个令牌，即将获取23个令牌
# 剩余3个令牌，即将获取26个令牌
# 等待4秒后重试
# 重试
# 剩余3个令牌，即将获取3个令牌
# 剩余20个令牌，即将获取25个令牌
# 等待1秒后重试
# 重试
# 剩余20个令牌，即将获取3个令牌
# 剩余22个令牌，即将获取28个令牌
# 等待1秒后重试
```



### 推荐阅读

[012-redis应用-05-限流【简单限流、漏斗限流】](https://www.cnblogs.com/bjlhx/p/12602875.html)

