#pragma once

struct KernelFastMutex : FAST_MUTEX {
    KernelFastMutex(const KernelFastMutex&) = delete;
    KernelFastMutex& operator=(const KernelFastMutex&) = delete;
    KernelFastMutex(KernelFastMutex&&) = delete;
    KernelFastMutex& operator=(KernelFastMutex&&) = delete;
    KernelFastMutex() {
        Init();
    }
    void Init();
    void Lock();
    void Unlock();
};

template<typename TLock>
struct AutoLock {
    AutoLock(TLock& lock) : _lock(lock) {
        lock.Lock();
    }
    ~AutoLock() {
        _lock.Unlock();
    }
private:
    TLock& _lock;
};