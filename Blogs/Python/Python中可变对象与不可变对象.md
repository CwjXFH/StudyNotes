Python中所有类型的值都是对象，这些对象分为可变对象与不可变对象两种：

+ 不可变类型

  `float`、`int`、`str`、`tuple`、`bool`、`frozenset`、`bytes`

  > tuple自身不可变，但可能包含可变元素，如：([3, 4, 5], 'tuple') 

+ 可变类型

  `list`、`dict`、`set`、`bytearray`、`自定义类型`



## +=操作符

+=操作符对应`__iadd__魔法方法`，对于**不可变对象**，`a+=b`和`a=a+b`等价，对于可变对象并不等价，`dict`和`set`不支持+=和+操作符。

```python
l1 = l2 = [1, 2, 3]
# 只有l1发生变化
# l1 = l1 + [4]
# l1和l2都发生变化，输出[1, 2, 3, 4, 5]
l1 += [4, 5]
print(l1)
print(l2)
```



## 浅拷贝 深拷贝

与赋值不同，拷贝（可能）会产生新的对象，可通过拷贝来避免不同对象间的相互影响。

在Python中，不可变对象，浅拷贝和深拷贝结果一样，都返回原对象：

```python
import copy


t1 = (1, 2, 3)
t2 = copy.copy(t1)
t3 = copy.deepcopy(t1)
print(t1 is t2) # True
print(t1 is t3) # True
print(id(t1), id(t2), id(t3)) # 输出相同值
```

对于可变对象，则会**产生新对象**，只是若原对象中存在可变属性/字段，则浅拷贝产生的对象的属性/字段引用原对象的属性/字段，深拷贝产生的对象和原对象则完全独立：

```python
l1 = [1, 2, 3]
l2 = l1.copy()
print(l1 is l2)  # False
l2[0] = 100
print(l1[0])  # 1
```



```python
import copy


class Id:
    def __init__(self, name):
        self.name = name


class Person:
    def __init__(self, id: Id):
        self.id = id


p1 = Person(Id("eason"))
p2 = copy.copy(p1)
print(p1 is p2)  # False
print(p1.id is p2.id)  # True
p2.id.name = "p2"
print(p1.id.name)  # p2

p3 = copy.deepcopy(p1)
print(p1 is p3)  # False
print(p1.id is p3.id)  # False
print(p1.id.name is p3.id.name)  # True，字符串不可变，这里name属性的地址一样
p3.id.name = "p3"
print(p1.id.name)  # 还是p2
```



Python中可使用以下几种方式进行浅拷贝：

+ 使用copy模块的copy方法

+ 可变类型切片

  ```python
  l1 = [1, 2, 3]
  l2 = l1[:]
  print(l1 is l2)  # False
  ```

+ 可变类型的copy方法

  ```python
  [].copy()
  {}.copy()
  set().copy()
  ```

+ 调用list, set, dict方法

  ```python
  l1 = [1, 2, 3]
  l2 = list(l1)
  l2[0] = 100
  print(l1[0])  # 1
  ```

+ 推导式

  列表、字典、集合推导式

  ```python
  class Person:
      def __init__(self, name):
          self.name = name
  
  
  l1 = [Person("l1")]
  l2 = [i for i in l1]
  print(l1 is l2)  # False
  print(l1[0] is l2[0])  # True
  
  s1 = {Person("s1")}
  s2 = {i for i in s1}
  print(s1 is s2)  # False
  
  ele1 = s1.pop()
  ele2 = s2.pop()
  print(ele1 is ele2)  # True
  ```

+ 解包

  ```python
  class Person:
      def __init__(self, name):
          self.name = name
  
  
  l1 = [Person("l1")]
  l2 = [*l1]
  print(l1 is l2)  # False
  l2[0].name = "l2"
  print(l1[0].name)  # l2
  ```

## 推荐阅读 

[Different behaviour for list.\_\_iadd\_\_ and list.\_\_add\_\_](https://stackoverflow.com/questions/9766387/different-behaviour-for-list-iadd-and-list-add)

[学习Python一年，这次终于弄懂了浅拷贝和深拷贝](https://mp.weixin.qq.com/s/_oUeyvlfra10GaLzZdNRrw)

[`copy` — Shallow and deep copy operations](https://docs.python.org/3/library/copy.html#module-copy)