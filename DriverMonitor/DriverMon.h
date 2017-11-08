#pragma once

#define DRIVER_PREFIX "DriverMon: "
const ULONG DRIVER_TAG = 'NMVD';

#include "Common.h"
#include "SimpleTable.h"
#include "CyclicBuffer.h"

// driver only declarations

const WCHAR DeviceSymLink[] = L"\\??\\DriverMon";
const WCHAR DeviceName[] = L"\\Device\\DriverMon";


extern "C"
NTSYSAPI
NTSTATUS NTAPI ObReferenceObjectByName(
    IN PUNICODE_STRING ObjectPath,
    IN ULONG Attributes,
    IN PACCESS_STATE PassedAccessState OPTIONAL,
    IN ACCESS_MASK DesiredAccess OPTIONAL,
    IN POBJECT_TYPE ObjectType,
    IN KPROCESSOR_MODE AccessMode,
    IN OUT PVOID ParseContext OPTIONAL,
    OUT PVOID *ObjectPtr
);

extern "C" POBJECT_TYPE* IoDriverObjectType;

void* __cdecl operator new(size_t size, POOL_TYPE type, ULONG tag = 0);
void __cdecl operator delete(void* p, size_t);

struct MonitoredDriver {
    WCHAR DriverName[64];
    PDRIVER_DISPATCH MajorFunction[IRP_MJ_MAXIMUM_FUNCTION + 1];
    PDRIVER_OBJECT DriverObject;
    PDRIVER_UNLOAD DriverUnload;
    PDEVICE_OBJECT DeviceObjects[4];
};

const int MaxMonitoredDrivers = 16;

struct DriverMonGlobals {
    MonitoredDriver Drivers[MaxMonitoredDrivers];
    SimpleTable<PVOID, PVOID, 256>* IrpCompletionTable;
    CyclicBuffer<SpinLock>* DataBuffer;
    PKEVENT NotifyEvent;
    short Count;
    bool IsMonitoring;
};

extern DriverMonGlobals globals;
