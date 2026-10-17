using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProcessEngine
{
    public class MainForm : Form
    {
        private ListBox lstProcesses = null!;
        private ComboBox cmbDlls = null!;
        private TextBox txtSearch = null!;
        private Button btnRefresh = null!;
        private Button btnInject = null!;
        private TextBox txtLogs = null!;
        private Label lblStatus = null!;

        private string dllsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlls");
        private List<ProcessInfo> allProcesses = new List<ProcessInfo>();

        private class ProcessInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Title { get; set; } = "";
            public string Arch { get; set; } = "";
            public override string ToString() => $"{Name} (PID: {Id}) [{Arch}] - {Title}";
        }

        public MainForm()
        {
            this.Text = "M.Engine - Advanced Process & DLL Injector (Admin Mode)";
            this.Width = 650;
            this.Height = 520;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeComponents();
            LoadProcesses();
            LoadDlls();
        }

        private void InitializeComponents()
        {
            // Arama Kutusu
            txtSearch = new TextBox { Location = new Point(12, 12), Size = new Size(380, 23), PlaceholderText = "Süreç ara..." };
            txtSearch.TextChanged += (s, e) => FilterProcesses(txtSearch.Text);
            this.Controls.Add(txtSearch);

            // Süreç Listesi
            lstProcesses = new ListBox { Location = new Point(12, 42), Size = new Size(380, 260) };
            this.Controls.Add(lstProcesses);

            // DLL Seçim Menüsü
            Label lblDll = new Label { Text = "Hedef DLL:", Location = new Point(405, 15), Size = new Size(215, 20) };
            this.Controls.Add(lblDll);

            cmbDlls = new ComboBox { Location = new Point(405, 42), Size = new Size(215, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbDlls);

            // Yenile Butonu
            btnRefresh = new Button { Text = "Listeleri Yenile", Location = new Point(405, 80), Size = new Size(215, 35) };
            btnRefresh.Click += (s, e) => { LoadProcesses(); LoadDlls(); Log("Listeler yenilendi."); };
            this.Controls.Add(btnRefresh);

            // Enjekte Et Butonu
            btnInject = new Button { Text = "DLL ENJEKTE ET", Location = new Point(405, 125), Size = new Size(215, 45), BackColor = Color.ForestGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnInject.Click += BtnInject_Click;
            this.Controls.Add(btnInject);

            // Log Paneli (Konsol)
            Label lblLog = new Label { Text = "İşlem Logları:", Location = new Point(12, 310), Size = new Size(380, 20) };
            this.Controls.Add(lblLog);

            txtLogs = new TextBox { Location = new Point(12, 335, Width = 608), Size = new Size(608, 95), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.Black, ForeColor = Color.Lime };
            this.Controls.Add(txtLogs);

            // Durum Çubuğu
            lblStatus = new Label { Text = "Durum: Hazır (Yönetici İzni Aktif)", Location = new Point(12, 440), Size = new Size(608, 25), ForeColor = Color.DarkSlateGray };
            this.Controls.Add(lblStatus);
        }

        private void LoadProcesses()
        {
            allProcesses.Clear();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(p.MainWindowTitle) || p.WorkingSet64 > 10000000) // Hafif filtre
                    {
                        allProcesses.Add(new ProcessInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Title = p.MainWindowTitle,
                            Arch = ProcessManager.GetProcessArchitecture(p)
                        });
                    }
                }
                catch { }
            }
            FilterProcesses("");
        }

        private void FilterProcesses(string filter)
        {
            lstProcesses.Items.Clear();
            foreach (var proc in allProcesses)
            {
                if (string.IsNullOrEmpty(filter) || proc.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    lstProcesses.Items.Add(proc);
                }
            }
        }

        private void LoadDlls()
        {
            cmbDlls.Items.Clear();
            if (!Directory.Exists(dllsDirectory)) Directory.CreateDirectory(dllsDirectory);

            foreach (var file in Directory.GetFiles(dllsDirectory, "*.dll"))
            {
                cmbDlls.Items.Add(Path.GetFileName(file));
            }
            if (cmbDlls.Items.Count > 0) cmbDlls.SelectedIndex = 0;
        }

        private void Log(string message)
        {
            txtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void BtnInject_Click(object? sender, EventArgs e)
        {
            if (lstProcesses.SelectedItem is not ProcessInfo selectedProc)
            {
                MessageBox.Show("Lütfen listeden bir süreç seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDlls.SelectedItem == null)
            {
                MessageBox.Show("dlls/ klasöründe DLL bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dllName = cmbDlls.SelectedItem.ToString() ?? "";
            string dllFullPath = Path.Combine(dllsDirectory, dllName);

            Log($"Enjeksiyon deneniyor -> Süreç: {selectedProc.Name} (PID: {selectedProc.Id}), DLL: {dllName}");

            bool success = MemoryEngine.InjectDll(selectedProc.Id, dllFullPath, out string errorDetails);
            if (success)
            {
                lblStatus.Text = $"Durum: Başarılı -> {dllName}";
                Log($"[BAŞARILI] {dllName}, PID {selectedProc.Id} sürecine yüklendi.");
                MessageBox.Show("DLL başarıyla enjekte edildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Durum: Enjeksiyon Başarısız!";
                Log($"[HATA] Başarısız. Detay: {errorDetails}");
                MessageBox.Show($"Enjeksiyon başarısız!\nDetay: {errorDetails}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
