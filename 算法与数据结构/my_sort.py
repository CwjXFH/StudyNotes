# _*_ coding=utf8 _*_
from typing import List


# region 冒泡/插入/选择 时间O(n^2)
def bubble_sort(arrs: List[int]) -> List[int]:
    if not arrs:
        return arrs

    arrs_len = len(arrs)
    for i in range(arrs_len):
        # 未排序区域
        for j in range(arrs_len - 1 - i):
            if arrs[j] > arrs[j + 1]:
                arrs[j], arrs[j + 1] = arrs[j + 1], arrs[j]

    return arrs


def insertion_sort(arrs: List[int]) -> List[int]:
    """插入排序，每次从未排序区域选择一个值插入到已排序区域的合适位置"""
    if not arrs:
        return arrs

    arrs_len = len(arrs)
    for i in range(1, arrs_len):
        val = arrs[i]
        # 已排序区域
        j = i - 1
        while j >= 0:
            if arrs[j] > val:
                arrs[j + 1] = arrs[j]
            else:
                # 不需要移动已排序区域的元素，直接将当前元素追加到已排序区域末尾
                break
            j -= 1
        arrs[j + 1] = val

    return arrs


def selection_sort(arrs: List[int]) -> List[int]:
    """选择排序，每次从未排序区域选择一个最小元素放到已排序区域末尾
    非稳定算法：[5, 3, 2, 5, 1]
    第一轮循环交换结束后，第一个元素5与最后一个元素1交换位置导致两个5的相对位置改变"""
    if not arrs:
        return arrs

    arrs_len = len(arrs)
    for i in range(arrs_len):
        min_index = i
        min_val = arrs[min_index]
        # 未排序区域
        for j in range(i, arrs_len):
            if arrs[j] < min_val:
                min_index = j
                min_val = arrs[min_index]
        # 排序
        arrs[i], arrs[min_index] = arrs[min_index], arrs[i]

    return arrs


# endregion

# region 快排/归并 时间O(nlogn)
def quick_sort(arrs: List[int], left: int = None, right: int = None) -> List[int]:
    """快排
    非稳定性算法：[9, 6, 8, 6]
    该数组中元素9与最后一个元素6的位置会互换，导致两个数字6的相对位置会改变"""
    if not arrs:
        return arrs

    arrs_len = len(arrs)
    left = 0 if not isinstance(left, int) else left
    right = arrs_len - 1 if not isinstance(right, int) else right

    if left < right:
        pivot = partition(arrs, left, right)
        quick_sort(arrs, left, pivot - 1)
        quick_sort(arrs, pivot + 1, right)

    return arrs


# def quick_sort_interation(arrs: List[int]) -> List[int]:
#     if not arrs:
#         return arrs
#
#     nums_len = len(arrs)


def partition(nums: List[int], left: int, right: int) -> int:
    pivot_val = nums[left]
    pivot = left + 1
    for i in range(left + 1, right + 1):
        if nums[i] < pivot_val:
            nums[i], nums[pivot] = nums[pivot], nums[i]
            pivot += 1

    nums[left], nums[pivot - 1] = nums[pivot - 1], nums[left]
    return pivot - 1


def merge_sort(arrs: List[int], start: int = None, end: int = None) -> List[int]:
    if not arrs:
        return arrs

    arrs_len = len(arrs)
    left = start if isinstance(start, int) else 0
    right = end if isinstance(end, int) else arrs_len - 1
    if left < right:
        mid = left + (right - left) // 2
        merge_sort(arrs, left, mid)
        merge_sort(arrs, mid + 1, right)
        merge(arrs, left, mid, right)
    return arrs


def merge(arrs: List[int], left: int, mid: int, right: int) -> List[int]:
    result = []
    i = left
    j = mid + 1
    while i <= mid or j <= right:
        if i <= mid and j <= right:
            if arrs[i] < arrs[j]:
                result.append(arrs[i])
                i += 1
            else:
                result.append(arrs[j])
                j += 1
        elif i <= mid:
            result.append(arrs[i])
            i += 1
        elif j <= right:
            result.append(arrs[j])
            j += 1

    arrs[left:right + 1] = result
    return arrs


# endregion

arrs = [2, 1, 3, 2, 9, 0]
# print(bubble_sort(arrs))
# print(insertion_sort(arrs))
# print(selection_sort(arrs))
# print(quick_sort(arrs))
print(merge_sort(arrs))
