# _*_ coding=utf8 _*_

class SimpleQueue:
    __ele_store = []
    __len = 0

    def __init__(self):
        pass

    def dequeue(self):
        if self.__len <= 0:
            return None
        self.__len -= 1
        return self.__ele_store.pop(0)

    def enqueue(self, ele):
        if ele is None:
            raise NameError
        self.__ele_store.append(ele)
        self.__len += 1

    @property
    def length(self):
        return self.__len
