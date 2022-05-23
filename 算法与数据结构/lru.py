from threading import RLock
from collections import namedtuple
from single_link import SingleLink


class _HashedSeq(list):
    """ This class guarantees that hash() will be called no more than once
        per element.  This is important because the lru_cache() will hash
        the key multiple times on a cache miss.
    """

    __slots__ = 'hashvalue'

    def __init__(self, tup, hash=hash):
        self[:] = tup
        self.hashvalue = hash(tup)

    def __hash__(self):
        return self.hashvalue


def _make_key(args, kwds, typed,
              kwd_mark=(object(),),
              fasttypes={int, str},
              tuple=tuple, type=type, len=len):
    """Make a cache key from optionally typed positional and keyword arguments

    The key is constructed in a way that is flat as possible rather than
    as a nested structure that would take more memory.

    If there is only a single argument and its data type is known to cache
    its hash value, then that argument is returned without a wrapper.  This
    saves space and improves lookup speed.

    """
    # All of code below relies on kwds preserving the order input by the user.
    # Formerly, we sorted() the kwds before looping.  The new way is *much*
    # faster; however, it means that f(x=1, y=2) will now be treated as a
    # distinct call from f(y=2, x=1) which will be cached separately.
    key = args
    if kwds:
        key += kwd_mark
        for item in kwds.items():
            key += item
    if typed:
        key += tuple(type(v) for v in args)
        if kwds:
            key += tuple(type(v) for v in kwds.values())
    elif len(key) == 1 and type(key[0]) in fasttypes:
        return key[0]
    return _HashedSeq(key)


def lru_cache(maxsize):
    def lru_cache_core(user_function):
        typed = False
        _CacheInfo = namedtuple(
            "CacheInfo", ["hits", "misses", "maxsize", "currsize"])
        # Constants shared by all lru cache instances:
        sentinel = object()  # unique object used to signal cache misses
        make_key = _make_key  # build a key from the function arguments
        PREV, NEXT, KEY, RESULT = 0, 1, 2, 3  # names for the link fields

        cache = {}
        hits = misses = 0
        full = False
        cache_get = cache.get  # bound method to lookup a key or return None
        cache_len = cache.__len__  # get cache size without calling len()
        lock = RLock()  # because linkedlist updates aren't threadsafe
        root = []  # root of the circular doubly linked list
        # root[:] = [root, root, None, None]  # initialize by pointing to self
        root[:] = [root, root, None, None]
        cache_node = SingleLink()

        if maxsize == 0:

            def wrapper(*args, **kwds):
                # No caching -- just a statistics update
                nonlocal misses
                misses += 1
                result = user_function(*args, **kwds)
                return result

        elif maxsize is None:

            def wrapper(*args, **kwds):
                # Simple caching without ordering or size limit
                nonlocal hits, misses
                key = make_key(args, kwds, typed)
                result = cache_get(key, sentinel)
                if result is not sentinel:
                    hits += 1
                    return result
                misses += 1
                result = user_function(*args, **kwds)
                cache[key] = result
                return result

        else:

            def wrapper(*args, **kwds):
                # Size limited caching that tracks accesses by recency
                nonlocal root, hits, misses, full
                key = make_key(args, kwds, typed)
                with lock:
                    link = cache_get(key)
                    if link is not None:
                        # Move the link to the front of the circular queue
                        # link_prev, link_next, _key, result = link
                        # link_prev[NEXT] = link_next
                        # link_next[PREV] = link_prev
                        # last = root[PREV]
                        # last[NEXT] = root[PREV] = link
                        # link[PREV] = last
                        # link[NEXT] = root
                        # hits += 1
                        #
                        # return result
                        cur = link.head
                        prev = None
                        while cur and cur.val:
                            if cur.val[0] == key:
                                if prev:
                                    link.remove_node(prev, cur)
                                    link.add_head((key, cur.val[1]))
                                    # cur.next = link.head
                                return cur.val[1]
                            else:
                                prev = cur
                                cur = cur.next
                        # return None
                    misses += 1
                result = user_function(*args, **kwds)
                with lock:
                    if key in cache:
                        # Getting here means that this same key was added to the
                        # cache while the lock was released.  Since the link
                        # update is already done, we need only return the
                        # computed result and update the count of misses.
                        pass
                    elif full:
                        # Use the old root to store the new key and result.
                        oldroot = root
                        oldroot[KEY] = key
                        oldroot[RESULT] = result
                        # Empty the oldest link and make it the new root.
                        # Keep a reference to the old key and old result to
                        # prevent their ref counts from going to zero during the
                        # update. That will prevent potentially arbitrary object
                        # clean-up code (i.e. __del__) from running while we're
                        # still adjusting the links.
                        root = oldroot[NEXT]
                        oldkey = root[KEY]
                        oldresult = root[RESULT]
                        root[KEY] = root[RESULT] = None
                        # Now update the cache dictionary.
                        del cache[oldkey]
                        # Save the potentially reentrant cache[key] assignment
                        # for last, after the root and links have been put in
                        # a consistent state.
                        cache[key] = oldroot
                    else:
                        # Put result in a new link at the front of the queue.
                        # last = root[PREV]
                        # link = [last, root, key, result]
                        # last[NEXT] = root[PREV] = cache[key] = link
                        # # Use the cache_len bound method instead of the len() function
                        # # which could potentially be wrapped in an lru_cache itself.
                        # full = (cache_len() >= maxsize)

                        cache_node.add_head((key, result))
                        cache[key] = cache_node
                        if cache_node.count > maxsize:
                            cache_node.remove_tail()

                return result

        def cache_info():
            """Report cache statistics"""
            with lock:
                return _CacheInfo(hits, misses, maxsize, cache_len())

        def cache_clear():
            """Clear the cache and cache statistics"""
            nonlocal hits, misses, full
            with lock:
                cache.clear()
                root[:] = [root, root, None, None]
                hits = misses = 0
                full = False

        wrapper.cache_info = cache_info
        wrapper.cache_clear = cache_clear
        return wrapper

    return lru_cache_core
