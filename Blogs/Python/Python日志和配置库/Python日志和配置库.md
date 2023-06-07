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

```tom
[person]
name = "eason"
age = 30
```

读取配置数据：

```python
from dataclasses import dataclass
from src.config.config import settings


@dataclass
class Person:
    name: str
    age: int


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





[loguru](https://github.com/Delgan/loguru)

日志

```shell
2023-06-07 21:06:04.154 | INFO     | __main__:<module>:3 - 一条日志信息
```

