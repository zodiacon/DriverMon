#include "pch.h"
#include "FastMutex.h"

void KernelFastMutex::Init() {
    ExInitializeFastMutex(this);
}
void KernelFastMutex::Lock() {
    ExAcquireFastMutex(this);
}
void KernelFastMutex::Unlock() {
    ExReleaseFastMutex(this);
}