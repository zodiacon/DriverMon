#pragma once

#include "FastMutex.h"

template<typename TKey, typename TValue, int Size>
struct SimpleTable {
    SimpleTable() : _count(0) {
        ::memset(_table, 0, sizeof(_table));
    }

    int Insert(const TKey& key, const TValue& value) {
        AutoLock<KernelFastMutex> lock(_lock);

        if (_count == Size)
            return -1;

        for (int i = 0; i < Size; i++) {
            if (_table[i].Key == TKey()) {
                _table[i].Key = key;
                _table[i].Value = value;
                _count++;
                return i;
            }
        }
        NT_ASSERT(false);
        return -1;
    }

    TValue Find(const TKey& key, int* index = nullptr) {
        AutoLock<KernelFastMutex> lock(_lock);

        for (int i = 0; i < Size; i++) {
            if (_table[i].Key == key) {
                if (index)
                    *index = i;
                return _table[i].Value;
            }
        }
        return TValue();
    }

    int Remove(const TKey& key) {
        AutoLock<KernelFastMutex> lock(_lock);
        for (int i = 0; i < Size; i++) {
            if (_table[i].Key == TKey()) {
                _table[i].Key = TKey();
                --_count;
                return i;
            }
        }
        return -1;
    }

    void RemoveAt(int index) {
        AutoLock<KernelFastMutex> lock(_lock);
        _table[index].Key = TKey();
        _count--;
    }

private:
    struct Item {
        TKey Key;
        TValue Value;
    };

    KernelFastMutex _lock;
    Item _table[Size];
    int _count;
};
