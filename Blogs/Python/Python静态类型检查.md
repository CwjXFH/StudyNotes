## 名词解释

类型提示：type hint，使用类型提示可提高代码可读性，并能获得IDE的只能提示，Python内置模块[typing](https://docs.python.org/3/library/typing.html)

静态类型检查：static type check，结合类型提示类型检查可在编译时进行类型安全检查，发现可能隐藏的BUG，使用[mypy](https://mypy-lang.org/)进行

存根文件：stub files，后缀pyi，通常只包含类、函数或模块的声明，可用于辅助类型检查，下面是[存根文件示例](https://github.com/python/typeshed/blob/main/stdlib/ctypes/util.pyi)：

```python
import sys

def find_library(name: str) -> str | None: ...

if sys.platform == "win32":
    def find_msvcrt() -> str | None: ...
```

> :information_desk_person: 很简单的脚本或者临时脚本没必要使用类型提示和静态类型检查



延迟解析类型注解

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

mypy.ini

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

[ruff](https://github.com/astral-sh/ruff)

[Ruff](https://beta.ruff.rs/docs/)是用rust写的（官方宣传）速度极快的Python linter

ruff.toml

```toml
line-length = 120
```





## 相关PEP

[PEP 484 - Type Hints](https://peps.python.org/pep-0484/)



## 推荐阅读

[from \_\_future\_\_ import annotations](https://stackoverflow.com/questions/61544854/from-future-import-annotations)

[When Do You Use an Ellipsis in Python?](https://realpython.com/python-ellipsis/)