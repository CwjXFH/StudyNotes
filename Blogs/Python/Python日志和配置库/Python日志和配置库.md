日志和配置是应用不可缺少的部分，本文用于介绍Python中的第三方日志和配置库。

[dynaconf](https://github.com/dynaconf/dynaconf) 

dynaconf是一个配置管理包，支持多种配置文件格式，如：toml、yaml、json、ini及环境变量等

```shell
pip install dynaconf

mkdir config
cd config
dynaconf init -f toml
```

命令生成的目录结构如下：

```shell
config
├── .gitignore
├── .secrets.toml
├── config.py
└── settings.toml
```

**.secrets.toml**用于存放敏感信息，默认被添加到**.gitignore**中，不会提交到代码仓库。

从环境变量读取配置：
```shell
# 环境变量前缀在config.py中设置
export PYDEMO_tag=dynaconf
```

```python
from src.config.config import settings
print(settings.tag) # dynaconf
```

**config.py**中可以指定读取的配置文件：

```python
from dynaconf import Dynaconf

settings = Dynaconf(
    # 环境变量前缀
    envvar_prefix="PYDEMO",
    # 可以指定多个配置文件，如：settings.dev.toml
    settings_files=['settings.toml', '.secrets.toml'],
)
```

在**settinngs.toml**中写入配置：
```toml
[person]
name = "eason"
age = 30
```

读取配置数据：

```python
from dataclasses import dataclass
from src.config.config import settings

print(settings.person) # 输出：{'name': 'eason', 'age': 30}
```

也可以定义具体的类型来绑定配置数据

```python
from dataclasses import dataclass
from src.config.config import settings


@dataclass
class Person:
    name: str
    age: int


p = Person(**settings.person)
print(p.name)
```

不同环境读取不同的配置：

```toml
[production]
person = { name = "prod", age = 100 }
[development]
person = { name = "dev", age = 100 }
```

config.py中设置**environments**的值是True
```python
settings = Dynaconf(
    envvar_prefix="PYDEMO",
    settings_files=['settings.toml', '.secrets.toml'],
    environments=True
)
```

环境变量**settings.ENV_FOR_DYNACONF**的值默认是development，dynaconf会读取[development]节点下的配置

```shell
# 设置环境变量，从production节点下读取配置
export ENV_FOR_DYNACONF=production
# unset ENV_FOR_DYNACONF
```



```python
from src.config.config import settings

print(settings.ENV_FOR_DYNACONF)

p = Person(**settings.person)
print(p.name)
```





[loguru](https://github.com/Delgan/loguru)

日志

```shell
2023-06-07 21:06:04.154 | INFO     | __main__:<module>:3 - 一条日志信息
```

