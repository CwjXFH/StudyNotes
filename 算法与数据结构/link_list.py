# _*_ coding: utf-8 _*_
# https://zhuanlan.zhihu.com/p/60057180


class LinkedListNode():
    """链表节点"""

    def __init__(self, item=None, next=None):
        self.item = item
        self.next = next


class SignleLiknedList():
    """单向链表"""

    def __init__(self):
        self.__header: LinkedListNode = LinkedListNode()
        self.__tail: LinkedListNode = self.__header
        self.__count: int = 0

    def add(self, node: LinkedListNode) -> None:
        if not node:
            return None
        self.__count += 1
        self.__tail.next = node
        self.__tail = node

    def remove(self, item) -> bool:
        if not item:
            return False
        prev = self.__header
        cur = self.__header.next
        while cur:
            if cur.item == item:
                if cur is self.__tail:
                    self.__tail = cur.next
                prev.next = cur.next
                self.__count -= 1
                return True
            prev = cur
            cur = cur.next
        return False

    def clear(self):
        self.__header.next = None
        self.__tail = self.__header
        self.__count = 0

    def to_list(self) -> list:
        if self.__count <= 0:
            return []
        cur = self.__header.next
        result = []
        while cur:
            result.append(cur.item)
            cur = cur.next
        return result

    def to_link(self, items: list) -> LinkedListNode:
        if not items:
            return None
        for i in items:
            self.add(LinkedListNode(i))

    @property
    def header(self):
        return self.__header.next

    @property
    def tail(self):
        return self.__tail

    @property
    def count(self):
        return self.__count
