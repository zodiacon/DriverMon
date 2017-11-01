#include "pch.h"

void* __cdecl operator new(size_t size, POOL_TYPE type, ULONG tag) {
    return ExAllocatePoolWithTag(type, size, tag);
}
void __cdecl operator delete(void* p, size_t) {
    ExFreePool(p);
}
