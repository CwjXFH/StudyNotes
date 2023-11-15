## 名词解释

类型提示：type hint，使用类型提示可提高代码可读性，并能获得IDE的智能提示，可以通过Python内置模块[typing](https://docs.python.org/3/library/typing.html)来完成

静态类型检查：static type check，结合类型提示类型检查可在编译时进行类型安全检查，发现可能隐藏的BUG，可以使用[mypy](https://mypy.readthedocs.io/en/stable/)进行

存根文件：stub files，后缀pyi，通常只包含类、函数或模块的声明，可用于辅助类型检查，下面是[存根文件示例](https://github.com/python/typeshed/blob/main/stdlib/ctypes/util.pyi)：

```python
import sys

def find_library(name: str) -> str | None: ...

if sys.platform == "win32":
    def find_msvcrt() -> str | None: ...
```

之前使用pb格式来描述数据时遇到的静态检查错误：[Get attr-defined & name-defined error when use protobuf](https://github.com/python/mypy/issues/14935)

> :information_desk_person: 很简单的脚本或者临时脚本没必要使用类型提示和静态类型检查



## 延迟解析类型注解

```python
class Person:
    @property
    def instance(self) -> Person:
        return self
```

以上代码编译器会报以下错误：

```python
    def instance(self) -> Person:
NameError: name 'Person' is not defined
```

在代码中添加模块引用：`from __future__ import annotations` 来实现[对注释的延迟计算](https://stackoverflow.com/questions/61544854/from-future-import-annotations)即可



## mypy

mypy可以使用mypy.ini作为[默认配置文件名](https://mypy.readthedocs.io/en/stable/config_file.html)，格式如下：

```ini
[mypy]
namespace_packages = True
explicit_package_bases = True

[mypy-dynaconf]
ignore_missing_imports = True

[mypy-pythoncom]
ignore_missing_imports = True

[mypy-win32com.*]
ignore_missing_imports = True

[mypy-simple_stack]
ignore_missing_imports = True
```



## ruff

[Ruff](https://beta.ruff.rs/docs/)是用rust写的，（官方宣传）速度极快的Python linter和code formatter工具

ruff可以使用ruff.toml作为[默认配置文件名](https://docs.astral.sh/ruff/settings/)，格式如下：

```toml
line-length = 120
```



## 使用示例

项目结构如下：

```shell
.
├── Pipfile
├── Pipfile.lock
├── README.md
├── docs
├── mypy.ini
├── resources
├── ruff.toml
├── samples
├── src
├── stubs
├── tests
└── venv_3112
```

执行命令：

```shell
# 静态类型检查
mypy ./src
# linter
ruff check ./src
# format
ruff format ./src
```



## 相关PEP

[PEP 484 - Type Hints](https://peps.python.org/pep-0484/)



## 推荐阅读

[from \_\_future\_\_ import annotations](https://stackoverflow.com/questions/61544854/from-future-import-annotations)

[When Do You Use an Ellipsis in Python?](https://realpython.com/python-ellipsis/)