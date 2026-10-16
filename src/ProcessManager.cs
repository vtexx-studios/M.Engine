using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProcessEngine
{
    public class ProcessTask
    {
        public string TargetName { get; set; }
        public IntPtr ProcessHandle { get; set; }
        public int ProcessId { get; set; }
        public bool IsActive => ProcessHandle != IntPtr.Zero;
    }

    public class ProcessManager
    {
        private readonly List<ProcessTask> _activeTasks = new List<ProcessTask>();

        public void ScanAndAttach(string processName)
        {
            Process[] localProcesses = Process.GetProcessesByName(processName);
            if (localProcesses.Length == 0)
            {
                Console.WriteLine($"[!] Hedef bulunamadı: {processName}");
                return;
            }

            foreach (var proc in localProcesses)
            {
                // Zaten ekli mi kontrol et
                if (_activeTasks.Exists(t => t.ProcessId == proc.Id)) continue;

                IntPtr hProcess = MemoryEngine.OpenTargetProcess(proc.Id);
                if (hProcess != IntPtr.Zero)
                {
                    _activeTasks.Add(new ProcessTask
                    {
                        TargetName = processName,
                        ProcessHandle = hProcess,
                        ProcessId = proc.Id
                    });
                    Console.WriteLine($"[+] Başarıyla bağlandı -> Süreç: {processName} (PID: {proc.Id})");
                }
                else
                {
                    Console.WriteLine($"[!] Bağlanılamadı (Yetki yetersiz olabilir) -> {processName} (PID: {proc.Id})");
                }
            }
        }

        public void RunMultiTaskOperation(Action<ProcessTask> operation)
        {
            // Çoklu görev (Parallel / Multithreading) ile tüm aktif süreçlerde eşzamanlı işlem yap
            Parallel.ForEach(_activeTasks, task =>
            {
                if (task.IsActive)
                {
                    try
                    {
                        operation(task);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Hata] PID {task.ProcessId} üzerinde işlem yapılırken hata: {ex.Message}");
                    }
                }
            });
        }

        public void Cleanup()
        {
            foreach (var task in _activeTasks)
            {
                if (task.IsActive)
                {
                    MemoryEngine.CloseProcess(task.ProcessHandle);
                    Console.WriteLine($"[-] Bağlantı kapatıldı -> PID: {task.ProcessId}");
                }
            }
            _activeTasks.Clear();
        }
    }
}
