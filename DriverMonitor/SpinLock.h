#pragma once

struct SpinLock {
	SpinLock() {
		Init();
	}

	void Init() {
		KeInitializeSpinLock(&_lock);
	}

	void Lock() {
		KeAcquireSpinLock(&_lock, &_oldIrql);
	}

	void Unlock() {
		KeReleaseSpinLock(&_lock, _oldIrql);
	}

private:
	KSPIN_LOCK _lock;
	KIRQL _oldIrql;
};
