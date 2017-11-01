#pragma once

#include "FastMutex.h"
#include "Common.h"

class CyclicBuffer {
public:
    CyclicBuffer() = default;
    ~CyclicBuffer() {
        Reset();
    }
    NTSTATUS Init(ULONG maxSize, POOL_TYPE pool, ULONG tag = 0);
    void Reset(bool freeBuffer = false);
    CyclicBuffer& Write(const CommonInfoHeader* data, ULONG size);
    ULONG Read(PUCHAR target, ULONG size);

private:
    PUCHAR _buffer{ nullptr };
    ULONG _currentReadOffset{ 0 }, _currentWriteOffset{ 0 };
    ULONG _maxSize;
    int _itemCount{ 0 };
    KernelFastMutex _lock;
};