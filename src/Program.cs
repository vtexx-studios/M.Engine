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

            if (args.Length > 0)
            {
                string filePath = args[0];
                if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".mengine", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[*] Özel uzantılı dosya algılandı: {Path.GetFileName(filePath)}");
                    // Burada dosya içerisindeki komutlar okunup işlenebilir
                }
            }

            // Örnek test döngüsü: Kullanıcıdan hedef süreç adı alalım (örn: notepad)
            Console.Write("Hedef süreç adını girin (Örn: notepad): ");
            string target = Console.ReadLine();

            if (!string.IsNullOrEmpty(target))
            {
                // Uzantı .exe ise temizle
                target = target.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"[*] {target} için tarama başlatılıyor...");
                manager.ScanAndAttach(target);

                Console.WriteLine("\n[*] Çoklu görev döngüsü çalışıyor. Çıkmak için bir tuşa basın...");
                
                // Örnek çoklu görev: Aktif süreçlerin durumunu periyodik kontrol et
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
