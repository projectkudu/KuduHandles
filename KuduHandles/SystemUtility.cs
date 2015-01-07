using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KuduHandles
{
    class SystemUtility
    {
        public static IEnumerable<Handle> GetHandles(int processId)
        {
            uint length = 0x10000;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                try { }
                finally
                {
                    ptr = Marshal.AllocHGlobal((int)length);
                }


                uint returnLength;
                NTSTATUS result;
                while ((result = NativeMethods.NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemHandleInformation, ptr, length, out returnLength)) ==
                       NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
                {
                    length = ((returnLength + 0xffff) & ~(uint)0xffff);
                    try { }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal((int)length);
                    }
                }

                if (result != NTSTATUS.STATUS_SUCCESS)
                    yield break;

                long handleCount = Marshal.ReadInt64(ptr);
                int offset = sizeof(long)*2;
                int size = Marshal.SizeOf(typeof(SYSTEM_HANDLE_ENTRY));
                for (int i = 0; i < handleCount; i++)
                {
                    var handleEntry =
                        (SYSTEM_HANDLE_ENTRY)Marshal.PtrToStructure(
                        IntPtr.Add(ptr, offset), typeof(SYSTEM_HANDLE_ENTRY));

                    if (handleEntry.OwnerProcessId == processId)
                    {
                        yield return new Handle(
                            handleEntry.OwnerProcessId,
                            handleEntry.Handle,
                            handleEntry.ObjectTypeNumber);
                    }

                    offset += size;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
