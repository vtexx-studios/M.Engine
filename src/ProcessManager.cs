using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessEngine
{
    public class ProcessManager
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

        public static string GetProcessArchitecture(Process proc)
        {
            if (!Environment.Is64BitOperatingSystem) return "x86";
            try
            {
                return IsWow64Process(proc.Handle, out bool isWow64) && isWow64 ? "x86" : "x64";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
