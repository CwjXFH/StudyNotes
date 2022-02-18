## Python MRO

> 文中代码基于Python3.7



对于Python中的多继承情况，运行时在搜索对象的属性或方法时，需要遵循一定的顺序规则，这个规则称为：**Method Resolution Order (MRO)**.



MRO规则可以总结为以下三句话：

+ In the multiple inheritance scenario, any specified attribute is searched first in the current class. If not found, the search continues into parent classes in **depth-first, left-right fashion without searching the same class twice**.

+ So, first it goes to super class (and its super classes)  given first in the list then second super class (and its super classes) , from left to right order. Then finally Object class, which is a super class for all classes.  

  > 这里的list指的是多个父类组成的list，如：
  >
  > class M(X,Y,Z):
  >
  > ​    pass
  >
  > list就是(X,Y,Z)

+ When in MRO we have a super class before subclass then it must be removed from that position in MRO.

  > 这一句和第一句对应起来看，一个类只被检索一次，所以基类要往后移



可以调用类型对象的`mro`方法或者`__mro__`属性来获取类型的MRO信息。



```python
class X:
    def hello(self):
        print('x')


class Y:
    def hello(self):
        print('y')
    
    def world(self):
        print('y_world')


class Z:
    def hello(self):
        print('z')


class A(X):
    def hello(self):
        print('a')


class B(Y,Z):
    def hello(self):
        print('b')


class M(B, A): 
    pass

print(M.mro())
print(M.__mro__)

# 输出：
# list类型
[<class '__main__.M'>, <class '__main__.B'>, <class '__main__.Y'>, <class '__main__.Z'>, <class '__main__.A'>, <class '__main__.X'>, <class 'object'>]
# tuple类型
(<class '__main__.M'>, <class '__main__.B'>, <class '__main__.Y'>, <class '__main__.Z'>, <class '__main__.A'>, <class '__main__.X'>, <class 'object'>)
```

MRO图示如下：

**goes to super class (and its super classes)  given first in the list then second super class (and its super classes) , from left to right order. Then finally Object class**

![](imgs/1.png)

**depth-first, left-right fashion without searching the same class twice**  ，得到MRO列表：`[<class '__main__.M'>, <class '__main__.B'>, <class '__main__.Y'>, <class '__main__.Z'>, <class '__main__.A'>, <class '__main__.X'>, <class 'object'>]`



---



B和A均继承自Z，M继承自B和A：

```python
class X:
    def hello(self):
        print('x')


class Y:
    def hello(self):
        print('y')
    
    def world(self):
        print('y_world')


class Z:
    def hello(self):
        print('z')


class A(X,Z):
    def hello(self):
        print('a')


class B(Y,Z):
    def hello(self):
        print('b')


class M(B, A): 
    pass

print(M.mro())

# 输出：
# [<class '__main__.M'>, <class '__main__.B'>, <class '__main__.Y'>, <class '__main__.A'>, <class '__main__.X'>, <class '__main__.Z'>, <class 'object'>]
```

MRO图示如下：

**goes to super class (and its super classes)  given first in the list then second super class (and its super classes) , from left to right order. Then finally Object class**

![](imgs/2.png)

**depth-first, left-right fashion without searching the same class twice**  ，得到MRO列表：`[<class '__main__.M'>, <class '__main__.B'>, <class '__main__.Y'>, <class '__main__.A'>, <class '__main__.X'>, <class '__main__.Z'>, <class 'object'>]`



---



```python
class X:
    def hello(self):
        print('x')


class Y:
    def hello(self):
        print('y')
    
    def world(self):
        print('y_world')


class Z:
    def hello(self):
        print('z')


class A(X,Z):
    def hello(self):
        print('a')


class B(Y,Z):
    def hello(self):
        print('b')


class M(B, A, Y): 
    pass

print(M.mro())

# 输出
# [<class '__main__.M'>, <class '__main__.B'>, <class '__main__.A'>, <class '__main__.Y'>, <class '__main__.X'>, <class '__main__.Z'>, <class 'object'>]
```

MRO图示如下：

 **goes to super class (and its super classes)  given first in the list then second super class (and its super classes) , from left to right order. Then finally Object class**

![](imgs/3.1.png)

这个MRO图可以继续简化：

**depth-first, left-right fashion without searching the same class twice**  

![](imgs/3.2.png)



得到MRO列表为`[<class '__main__.M'>, <class '__main__.B'>, <class '__main__.A'>, <class '__main__.Y'>, <class '__main__.X'>, <class '__main__.Z'>, <class 'object'>]`



---



**When in MRO we have a super class before subclass then it must be removed from that position in MRO**



下面是一个会报错的示例：

```python
class A:
    def process(self):
        print('A process()')


class B(A):
    def process(self):
        print('B process()')


class M(A, B):
    pass

print(M.mro())

# 输出：
# TypeError: Cannot create a consistent method resolution
# order (MRO) for bases A, B
```

MRO图示：

![](imgs/4.png)

如果一个方法或属性同时存在与B和A，应为M直接继承B又直接继承A，那么通过M来调用时就不知道是该从B中还是A中获取这个方法或属性了，干脆就报错吧。我觉得MRO顺序应该为：M->B->A->object。

## 推荐阅读

[Python Multiple Inheritance](https://www.programiz.com/python-programming/multiple-inheritance)  

[Method Resolution Order (MRO) in Python](http://www.srikanthtechnologies.com/blog/python/mro.aspx)  