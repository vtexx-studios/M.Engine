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

        // Method 1: Standard LoadLibraryA Injection (Most Common)
        public static bool InjectStandard(int processId, string dllPath, out string errorDetails)
        {
            errorDetails = string.Empty;
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                errorDetails = $"OpenProcess Failed. Error Code: {Marshal.GetLastWin32Error()}";
                return false;
            }

            try
            {
                IntPtr pDllPath = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (pDllPath == IntPtr.Zero)
                {
                    errorDetails = $"VirtualAllocEx Failed. Error Code: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
                if (!WriteProcessMemory(hProcess, pDllPath, bytes, bytes.Length, out _))
                {
                    errorDetails = $"WriteProcessMemory Failed. Error Code: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibrary = GetProcAddress(hKernel32, "LoadLibraryA");
                if (pLoadLibrary == IntPtr.Zero)
                {
                    errorDetails = "LoadLibraryA address could not be resolved.";
                    return false;
                }

                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibrary, pDllPath, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    errorDetails = $"CreateRemoteThread Failed (Anti-cheat/Permissions block): {Marshal.GetLastWin32Error()}";
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

        // Method 2: Alternative Injection Simulation / Stub Check
        public static bool InjectAlternative(int processId, string dllPath, out string errorDetails)
        {
            // İleride farklı bir enjeksiyon yöntemi (örn: QueueUserAPC veya Manual Mapping altyapısı) eklemek için şablon
            // Şimdilik standart yönteme yönlendiriyor veya alternatif handle açma testleri yapıyor.
            return InjectStandard(processId, dllPath, out errorDetails);
        }

        // DLL İçerisindeki Menü Açılış Tuşunu Tarama Fonksiyonu
        public static string ScanDllForMenuKey(string dllPath)
        {
            try
            {
                if (!File.Exists(dllPath)) return "DLL file not found.";
                byte[] fileBytes = File.ReadAllBytes(dllPath);

                // Yaygın sanal tuş (Virtual Key) kodları veya ASCII string taraması
                // Örneğin INSERT (0x2D / 45), DELETE (0x2E), HOME (0x24), F1-F12 (0x70-0x7B)
                foreach (byte b in fileBytes)
                {
                    if (b == 0x2D) return "INSERT Key (Common Menu Toggle)";
                    if (b == 0x24) return "HOME Key";
                    if (b == 0x70) return "F1 Key";
                    if (b == 0x75) return "F6 Key";
                    if (b == 0x77) return "F8 Key";
                }

                // Alternatif string tabanlı arama
                string content = Encoding.ASCII.GetString(fileBytes);
                if (content.Contains("Insert", StringComparison.OrdinalIgnoreCase)) return "INSERT (Found in strings)";
                if (content.Contains("Home", StringComparison.OrdinalIgnoreCase)) return "HOME (Found in strings)";
                if (content.Contains("Menu", StringComparison.OrdinalIgnoreCase)) return "Detected 'Menu' keyword inside DLL";

                return "Unknown / No standard hotkey signature found";
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return "Scan failed.";
            }
        }
    }
}
