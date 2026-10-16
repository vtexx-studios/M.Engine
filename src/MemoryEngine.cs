using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessEngine
{
    public static class MemoryEngine
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

        public static IntPtr OpenTargetProcess(int processId)
        {
            return OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        }

        public static bool ReadMemory<T>(IntPtr hProcess, IntPtr address, out T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            bool success = ReadProcessMemory(hProcess, address, buffer, size, out var bytesRead);
            
            if (success && bytesRead.ToInt32() == size)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return true;
            }

            value = default;
            return false;
        }

        public static bool WriteMemory<T>(IntPtr hProcess, IntPtr address, T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return WriteProcessMemory(hProcess, address, buffer, size, out _);
        }

        public static void CloseProcess(IntPtr hProcess)
        {
            CloseHandle(hProcess);
        }
    }
}
