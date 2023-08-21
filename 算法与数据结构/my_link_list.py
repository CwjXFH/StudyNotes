# Definition for singly-linked list.
class ListNode:
    def __init__(self, x):
        self.val = x
        self.next = None

    def __eq__(self, other):
        return False if not other else id(self) == id(other)


class Solution:
    @staticmethod
    def detectCycle(head: ListNode) -> bool:
        if not head:
            return None
        slow = head
        fast = head
        while fast and fast.next:
            slow = slow.next
            fast = fast.next.next
            if id(slow) == id(fast):
                break
        if not fast or not fast.next:
            return None
        slow = head
        while True:
            if id(slow) == id(fast):
                break
            slow = slow.next
            fast = fast.next
        return fast


head = ListNode(0)
head.next = ListNode(1)
# head.next.next = head
head.next.next = ListNode(2)
head.next.next.next = ListNode(3)
# head.next.next.next.next = head.next
head.next.next.next.next = ListNode(4)
head.next.next.next.next.next = head.next

r = Solution.detectCycle(head)
print(r)
