#### everythin is object
```python
class Person():
    # 通过类或实例调用
    tag = 'tag'

    def __init__(self, name):
        self.name = name

    def info(self):
        print(p.name, p.tag, id(p.tag))

    def show_id(self):
        print(id(self.__class__), id(Person))


# everythin is object
# Person类型及其实例都是对象：一个变量名是Person，一个变量名是P
p = Person('xfh')
p.info()
p.tag = '12'
p.info()
print(Person.tag, id(Person.tag))
p.show_id()

# 这里有点儿类似JavaScript中的原型继承了
# 删除了实例p上的attribute:tag后，p.tag=Person.tag
del p.tag
print(Person.tag, id(Person.tag))
p.info()

```



#### python -O file.py

```python
# 在使用python -O 执行脚本时，__debug__的值为False，且忽略assert语句

if __debug__:
    print('debug')
else:
    print('optimize')

# assert False, 'assert false'

```



#### hashable

```python
"""An object is hashable if it has a hash value which never changes during its lifetime (it needs a __hash__() method), and can be compared to other objects (it needs an __eq__() method). Hashable objects which compare equal must have the same hash value.

Hashability makes an object usable as a dictionary key and a set member, because these data structures use the hash value internally.

All of Python’s immutable built-in objects are hashable, while no mutable containers (such as lists or dictionaries) are. Objects which are instances of user-defined classes are hashable by default; they all compare unequal, and their hash value is their id()."""

class Person():
    def __init__(self, name, age):
        self.name = name
        self.age = age

    def __hash__(self):
        return hash(self.name+str(self.age))

    def __eq__(self, other):
        if other is None:
            return false
        if hash(self) != hash(other):
            return False
        return self.name == other.name and self.age == other.age

```



#### UTC To Local

```python
import datetime

def utc_to_local(utc_str: str) -> datetime.datetime:
    utc_format = "%Y-%m-%dT%H:%M:%SZ"
    utc_mill_format = "%Y-%m-%dT%H:%M:%S.%fZ"
    try:
        utc_time: datetime.datetime = datetime.datetime.strptime(utc_str, utc_format)
    except ValueError:
        utc_time: datetime.datetime = datetime.datetime.strptime(utc_str, utc_mill_format)
    localtime: datetime.datetime = utc_time + datetime.timedelta(hours=8)
    return localtime.strftime('%Y-%m-%d %H:%M:%S.%f')
```

