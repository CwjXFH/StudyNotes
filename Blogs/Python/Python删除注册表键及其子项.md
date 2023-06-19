Python中可以使用[winreg](https://docs.python.org/3.11/library/winreg.html)模块中的[DeleteKey](https://docs.python.org/3.11/library/winreg.html#winreg.DeleteKey)方法来删除注册表中的键，但该方法不能删除包含子键的键。所以，在删除某个键时要先删除它的所有子键，这里借助[栈结构](./Python实现一个简单的栈结构.md)通过循环迭代的方式来获取注册表键的所有子键及子键的子键。完整代码如下：

```python
# 注册表相关

# import os
#
# print(os.getlogin())
#
# import ctypes
#
# if not ctypes.windll.shell32.IsUserAnAdmin():
#     raise PermissionError("非管理员权限")

import winreg

from loguru import logger

from simple_stack import SimpleStack

# 注册表键不存在子键错误码
# https://bugs.python.org/file7326/winerror.py
_ERROR_NO_MORE_ITEMS = 259
# 删除该键下的所有子键
_root_key = "注册表键名" # 如：Software\Microsoft\Office

_key_stack = SimpleStack()
_key_stack.push(_root_key)
_key_name_stack = SimpleStack()


def _open_registry(key_name):
    key = winreg.OpenKey(winreg.HKEY_CURRENT_USER                # 根据实际需要更改
                         , key_name                              # 根据实际需要更改
                         , reserved=0
                         , access=winreg.KEY_ALL_ACCESS)
    return key


def _get_sub_keys():
    """获取键的所有直接子键"""
    i = 0
    key_name = _key_stack.pop()
    if key_name is None:
        return
    key: winreg.HKEYType | None = None
    try:
        key = _open_registry(key_name)
        while True:
            sub_key = winreg.EnumKey(key, i)
            sub_key_name = fr"{key_name}\{sub_key}"
            _key_stack.push(sub_key_name)
            _key_name_stack.push(sub_key_name)
            i += 1
    except OSError as err:
        if err.winerror != _ERROR_NO_MORE_ITEMS:
            logger.exception("获取注册表键异常：{err}", err=err)
    finally:
        if key:
            key.Close()


def del_registry_key():
    """删除注册表中的键"""
    
    # 迭代获取键的所有子键及子键的子键......
    while len(_key_stack) > 0:
        _get_sub_keys()

    key_name = _key_name_stack.pop()
    while key_name is not None:
        key: winreg.HKEYType | None = None
        try:
            key = _open_registry(key_name)
            winreg.DeleteKey(key, "")
            logger.info("已删除注册表键：{key_name}", key_name=key_name)
        except Exception as err:
            logger.exception("删除注册表键异常：{err}", err=err)
        finally:
            if key:
                key.Close()
        key_name = _key_name_stack.pop()
```

> :warning: 需用管理员权限来运行脚本
