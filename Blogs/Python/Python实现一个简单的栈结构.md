基于**list**来实现一个栈：
```python
class SimpleStack:
    def __init__(self):
        self.__stack: list = []

    def __iter__(self):
        return SimpleStackIterator(self)

    def __len__(self):
        return len(self.__stack)

    def __getitem__(self, index):
        if index >= len(self.__stack):
            return None
        return self.__stack[index]

    def pop(self):
        if len(self.__stack) <= 0:
            return None
        return self.__stack.pop()

    def push(self, item):
        return self.__stack.append(item)
```

定义一个栈的迭代器，实现对栈进行迭代，但不弹出栈中的元素

```python
class SimpleStackIterator:
    def __init__(self, iterable: SimpleStack):
        self.__iterable = iterable
        self.__len = len(iterable)
        self.__current = 0

    def __iter__(self):
        return self

    def __next__(self):
        while True:
            if self.__current >= self.__len:
                raise StopIteration
            ele = self.__iterable[self.__current]
            self.__current += 1
            return ele
```

具体使用：

```python
stack_size = 5
stack = SimpleStack()

for i in range(stack_size):
    stack.push(i)

for i in iter(stack):
    print(i)

for i in range(stack_size):
    ele = stack.pop()
    print(ele)
```

