using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessEngine
{
    public static class MemoryEngine
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;

        public static bool InjectDll(int processId, string dllPath, out string errorDetails)
        {
            errorDetails = string.Empty;
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                errorDetails = $"OpenProcess Hatası: {Marshal.GetLastWin32Error()}";
                return false;
            }

            try
            {
                IntPtr pDllPath = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (pDllPath == IntPtr.Zero)
                {
                    errorDetails = $"VirtualAllocEx Hatası: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
                if (!WriteProcessMemory(hProcess, pDllPath, bytes, bytes.Length, out _))
                {
                    errorDetails = $"WriteProcessMemory Hatası: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibrary = GetProcAddress(hKernel32, "LoadLibraryA");
                if (pLoadLibrary == IntPtr.Zero)
                {
                    errorDetails = "LoadLibraryA adresi bulunamadı.";
                    return false;
                }

                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibrary, pDllPath, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    errorDetails = $"CreateRemoteThread Hatası (Anti-cheat engeli olabilir): {Marshal.GetLastWin32Error()}";
                    return false;
                }

                CloseHandle(hThread);
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
    }
}
