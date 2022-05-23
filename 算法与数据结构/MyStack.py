# _*_ coding=utf8 _*_

class SequenceStack:
    __ele_store = []
    __len = 0

    def __init__(self):
        pass

    def push(self, ele):
        if ele is None:
            raise NameError
        self.__ele_store.append(ele)
        self.__len += 1

    def pop(self):
        if self.__len <= 0:
            return None
        ele = self.__ele_store.pop(self.__len - 1)
        self.__len -= 1
        return ele

    @property
    def length(self):
        return self.__len
