using System;
using System.IO;

namespace ProcessEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("         M.Engine v1.0 - Core            ");
            Console.WriteLine("   Multi-Process & Memory Engine         ");
            Console.WriteLine("=========================================\n");

            ProcessManager manager = new ProcessManager();
            string targetProcess = "";

            // Eğer özel .mengine uzantılı bir dosyaya çift tıklandıysa veya argüman olarak verildiyse
            if (args.Length > 0)
            {
                string filePath = args[0];
                if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".mengine", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[*] Özel uzantılı profil okunuyor: {Path.GetFileName(filePath)}");
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("target=", StringComparison.OrdinalIgnoreCase))
                        {
                            targetProcess = line.Split('=')[1].Trim();
                        }
                    }
                }
            }

            // Eğer dosyadan hedef bulunamadıysa kullanıcıdan iste
            if (string.IsNullOrEmpty(targetProcess))
            {
                Console.Write("Hedef süreç adını girin (Örn: notepad): ");
                targetProcess = Console.ReadLine();
            }

            if (!string.IsNullOrEmpty(targetProcess))
            {
                targetProcess = targetProcess.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"[*] '{targetProcess}' için çoklu görev taraması başlatılıyor...");
                manager.ScanAndAttach(targetProcess);

                Console.WriteLine("\n[*] Motor aktif. Çıkmak için bir tuşa basın...");
                
                manager.RunMultiTaskOperation(task =>
                {
                    Console.WriteLine($"[Bilgi] Aktif Görev Takipte -> PID: {task.ProcessId}");
                });

                Console.ReadKey();
                manager.Cleanup();
            }
            else
            {
                Console.WriteLine("[!] Geçersiz hedef.");
            }
        }
    }
}
