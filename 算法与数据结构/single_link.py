# _*_ encoding=utf8 _*_
from typing import List


class LinkNode:
    def __init__(self, val=None, next=None):
        self.val = val
        self.next = next


class SingleLink:
    def __init__(self):
        self.__count = 0
        self.__head = LinkNode()
        self.__tail = self.__head

    def add_tail(self, val):
        self.__tail.next = LinkNode(val)
        self.__tail = self.__tail.next
        self.__count += 1

    def add_head(self, val):
        node = LinkNode(val)
        node.next = self.__head.next
        self.__head.next = node
        if self.__tail == self.__head:
            self.__tail = node
        self.__count += 1

    def remove(self, val):
        if self.__count <= 0:
            return False
        cur = self.__head.next
        prev = self.__head
        while cur:
            if cur.val == val:
                prev.next = cur.next
                if cur == self.__tail:
                    self.__tail == prev
                self.__count -= 1
                return True
            else:
                prev = cur
                cur = cur.next
        return False

    def remove_tail(self):
        if self.__count <= 0:
            return False
        prev = self.__head
        cur = self.__head.next
        while cur:
            if not cur.next:
                prev.next = None
                self.__count -= 1
                return True
            else:
                prev = cur
                cur = cur.next
        return False

    def remove_node(self, prev, cur):
        if not prev or not cur:
            return False
        self.__count -= 1
        prev.next = cur.next

    def to_link(self, eles: List[any]):
        if not eles:
            return None
        for ele in eles:
            self.add_tail(ele)

    def to_list(self):
        if self.__count <= 0:
            return []
        result = []
        cur = self.__head.next
        while cur:
            result.append(cur.val)
            cur = cur.next
        return result

    @property
    def head(self):
        return self.__head.next

    @property
    def count(self):
        return self.__count
