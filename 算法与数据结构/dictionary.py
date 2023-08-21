# _*_coding: utf-8 _*_
# https://mp.weixin.qq.com/s?__biz=MzUyOTk2MTcwNg==&mid=2247486877&idx=1&sn=2c8a3021550666c95007b3f16ea231a9&chksm=fa584a18cd2fc30eecbb156372bce3fa929e39f935efee1d4f6c0b3cbc72ea6cb735428a9630&mpshare=1&scene=1&srcid=0926bBiOoZf8H1BplT1khvpH&sharer_sharetime=1601095103197&sharer_shareid=266dc9451fd28ecaad4697cc057771d2&key=0a19845a51c58415f5e23037fbc78c748ace9ba0a5aaff408c5faabf53c5a340c940294dabd0b4e78af014a0cb939f4ca5f86df138f966f07e9a9ccd19418f9bd5524d1aa7ac2fc257a8ae781dcd18f3335574dcc958120fe0333d34318196127af61fdf3198a7173eaf00a79345938a05215220713204bbbee84e6351423d92&ascene=1&uin=MTI5MDA0NDAwOA%3D%3D&devicetype=Windows+10&version=62070158&lang=zh_CN&exportkey=AZCTq18RYFqS%2BBII%2Bu4pvpU%3D&pass_ticket=Nmlj6bXbHC6L8HV47I7Ld1x7IUpyK6Oqb6DGKPU37iJB0Q8koxVXwnDD%2BCT1aBJh&wx_header=0
from src.data_structures.link_list import (SignleLiknedList, LinkedListNode)


class MyDict():
    """自己实现字典结构，非线程安全"""
    __factor = 0.75

    def __init__(self, value_type: type, capacity: int):
        if value_type is None or type(value_type) is not type:
            raise ValueError(value_type)
        if capacity <= 0:
            raise ValueError(capacity)
        self.__value_type = value_type
        self.__capacity = capacity
        self.__count = 0
        self.__buckets = [None for _ in range(0, capacity)]

    @property
    def count(self) -> int:
        return self.__count

    def add(self, key, value) -> None:
        __entry = _Entry(0, '', '')
        if type(value) is not self.__value_type:
            raise TypeError
        key_hash = hash(key)
        bucket_no = key_hash % self.__capacity
        if self.__buckets[bucket_no] is None:
            self.__buckets[bucket_no] = SignleLiknedList()
        entry_list: SignleLiknedList = self.__buckets[bucket_no]

        node = self.__get_entry(key)
        if node is None:
            entry = _Entry(key_hash, key, value)
            entry_list.add(LinkedListNode(entry))
            self.__count += 1
        else:
            node.item.value = value

        if entry_list.count > self.__factor * self.__capacity:
            self.__reset()

    def get(self, key: str):
        node = self.__get_entry(key)
        return None if node is None else node.item.value

    def remove(self, key):
        key_hash = hash(key)
        bucket_no = key_hash % self.__capacity
        entry_list = self.__buckets[bucket_no]
        node = self.__get_entry(key)
        remove_result = entry_list.remove(node)
        if remove_result:
            self.__count -= 1
        return remove_result

    def __reset(self):
        old_cap = self.__capacity
        self.__capacity = 2 * self.__capacity
        new_buckets = [None for _ in range(0, self.__capacity)]
        for i in range(0, old_cap):
            entry_list = self.__buckets[i]
            if entry_list is None:
                continue
            for node in entry_list.nodes():
                new_bucket_no = node.item.hash_code % self.__capacity
                if new_buckets[new_bucket_no] is None:
                    new_buckets[new_bucket_no] = SignleLiknedList()
                new_entry_list = new_buckets[new_bucket_no]
                new_entry_list.add(node)
        self.__buckets = new_buckets

    def __get_entry(self, key) -> LinkedListNode:
        if key is None:
            raise ValueError
        if type(key) is not str:
            raise TypeError
        key_hash = hash(key)
        bucket_no = key_hash % self.__capacity
        entry_list = self.__buckets[bucket_no]
        if entry_list is None:
            return None
        return next(
            (
                node
                for node in entry_list.nodes()
                if node.item.hash_code == key_hash and node.item.key == key
            ),
            None,
        )


class _Entry():
    def __init__(self, hash_code: int, key: str, value):
        self.__hash_code = hash_code
        self.__key = key
        self.__value = value

    @property
    def key(self):
        return self.__key

    @property
    def value(self):
        return self.__value

    @value.setter
    def value(self, value):
        self.__value = value

    @property
    def hash_code(self):
        return self.__hash_code
