

Python中所有类型的值都是对象，这些对象分为可变对象与不可变对象两种：

+ 不可变类型

  `float`、`int`、`str`、`tuple`、`bool`、`frozenset`、`bytes`

  > tuple自身不可变，但可能包含可变元素，如：([3, 4, 5], 'tuple') 

+ 可变类型

  `list`、`dict`、`set`、`bytearray`



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

可通过拷贝来避免不同对象间的相互影响



## 推荐阅读 

[Different behaviour for list.\_\_iadd\_\_ and list.\_\_add\_\_](https://stackoverflow.com/questions/9766387/different-behaviour-for-list-iadd-and-list-add)

[学习Python一年，这次终于弄懂了浅拷贝和深拷贝](https://mp.weixin.qq.com/s/_oUeyvlfra10GaLzZdNRrw)

[`copy` — Shallow and deep copy operations](https://docs.python.org/3/library/copy.html#module-copy)