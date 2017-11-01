#include "pch.h"
#include "CyclicBuffer.h"
#include "DriverMon.h"

NTSTATUS CyclicBuffer::Init(ULONG maxSize, POOL_TYPE pool, ULONG tag) {
    if (_buffer != nullptr) {
        return STATUS_TOO_LATE;
    }

    _maxSize = maxSize;
    _buffer = static_cast<PUCHAR>(ExAllocatePoolWithTag(pool, maxSize, tag));
    if (_buffer == nullptr) {
        KdPrint((DRIVER_PREFIX "Failed to allocate %d bytes\n", maxSize));
        return STATUS_INSUFFICIENT_RESOURCES;
    }

    memset(_buffer, 0, _maxSize);

    _lock.Init();

    return STATUS_SUCCESS;
}

void CyclicBuffer::Reset(bool freeBuffer) {
    AutoLock<KernelFastMutex> locker(_lock);

    if (freeBuffer && _buffer) {
        ExFreePool(_buffer);
        _buffer = nullptr;
        _maxSize = 0;
    }
    _currentWriteOffset = _currentReadOffset = 0;
}

CyclicBuffer& CyclicBuffer::Write(const CommonInfoHeader* data, ULONG size) {
    AutoLock<KernelFastMutex> locker(_lock);
    if (_buffer == nullptr)
        return *this;
    if (_currentWriteOffset < _currentReadOffset && _currentWriteOffset + size > _currentReadOffset) {
        KdPrint((DRIVER_PREFIX "Cyclic buffer overrun\n"));
        // data loss
        _currentReadOffset = _currentWriteOffset;
        _itemCount = 0;
    }
    if (_currentWriteOffset + size > _maxSize) {
        if (_currentWriteOffset + sizeof(CommonInfoHeader) <= _maxSize) {
            memset(_buffer + _currentWriteOffset, 0, sizeof(CommonInfoHeader));
        }
        _currentWriteOffset = 0;
    }
    _itemCount++;
    RtlCopyMemory(_buffer + _currentWriteOffset, data, size);
    _currentWriteOffset += size;
    return *this;
}

ULONG CyclicBuffer::Read(PUCHAR target, ULONG size) {
    AutoLock<KernelFastMutex> locker(_lock);

    if (_buffer == nullptr)
        return 0;

    ULONG total = 0;
    while (total < size && _itemCount > 0) {
        auto header = reinterpret_cast<CommonInfoHeader*>(_buffer + _currentReadOffset);
        auto itemSize = header->Size;
        if (itemSize == 0) {
            _currentReadOffset = 0;
            continue;
        }

        NT_ASSERT(itemSize >= sizeof(CommonInfoHeader));

        if (itemSize + total > size)
            break;

        RtlCopyMemory(target, header, itemSize);
        _itemCount--;
        total += itemSize;
        target += itemSize;

        _currentReadOffset += itemSize;
        if (_currentReadOffset + sizeof(CommonInfoHeader) > _maxSize) {
            _currentReadOffset = 0;
        }
        else if (_itemCount > 0) {
            header = reinterpret_cast<CommonInfoHeader*>(_currentReadOffset + _buffer);
            if (header->Size == 0)
                _currentReadOffset = 0;
        }
    }

    return total;
}
