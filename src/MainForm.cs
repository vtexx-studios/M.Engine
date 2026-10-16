using System;
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
        private Button btnRefresh = null!;
        private Button btnInject = null!;
        private Label lblStatus = null!;

        private string dllsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlls");

        public MainForm()
        {
            this.Text = "M.Engine - Advanced Process & DLL Injector";
            this.Width = 600;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeComponents();
            LoadProcesses();
            LoadDlls();
        }

        private void InitializeComponents()
        {
            lstProcesses = new ListBox
            {
                Location = new Point(12, 12),
                Size = new Size(350, 350)
            };
            this.Controls.Add(lstProcesses);

            cmbDlls = new ComboBox
            {
                Location = new Point(375, 12),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbDlls);

            btnRefresh = new Button
            {
                Text = "Listeyi Yenile",
                Location = new Point(375, 50),
                Size = new Size(200, 30)
            };
            btnRefresh.Click += (s, e) => { LoadProcesses(); LoadDlls(); };
            this.Controls.Add(btnRefresh);

            btnInject = new Button
            {
                Text = "DLL Enjekte Et",
                Location = new Point(375, 90),
                Size = new Size(200, 40),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnInject.Click += BtnInject_Click;
            this.Controls.Add(btnInject);

            lblStatus = new Label
            {
                Text = "Durum: Hazır. dlls/ klasörüne DLL ekleyebilirsiniz.",
                Location = new Point(12, 375),
                Size = new Size(560, 25),
                ForeColor = Color.DarkSlateGray
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadProcesses()
        {
            lstProcesses.Items.Clear();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        lstProcesses.Items.Add($"{p.ProcessName} (PID: {p.Id}) - {p.MainWindowTitle}");
                    }
                }
                catch { }
            }
        }

        private void LoadDlls()
        {
            cmbDlls.Items.Clear();
            if (!Directory.Exists(dllsDirectory))
            {
                Directory.CreateDirectory(dllsDirectory);
            }

            string[] dllFiles = Directory.GetFiles(dllsDirectory, "*.dll");
            foreach (var file in dllFiles)
            {
                cmbDlls.Items.Add(Path.GetFileName(file));
            }

            if (cmbDlls.Items.Count > 0)
                cmbDlls.SelectedIndex = 0;
        }

        private void BtnInject_Click(object? sender, EventArgs e)
        {
            if (lstProcesses.SelectedItem == null)
            {
                MessageBox.Show("Lütfen listeden hedef bir süreç seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDlls.SelectedItem == null)
            {
                MessageBox.Show("dlls/ klasöründe enjekte edilecek bir DLL bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedProcessText = lstProcesses.SelectedItem.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedProcessText)) return;

            int pidStart = selectedProcessText.LastIndexOf("PID: ") + 5;
            int pidEnd = selectedProcessText.LastIndexOf(")");
            if (pidStart < 5 || pidEnd <= pidStart) return;

            int pid = int.Parse(selectedProcessText.Substring(pidStart, pidEnd - pidStart));

            string dllName = cmbDlls.SelectedItem.ToString() ?? "";
            string dllFullPath = Path.Combine(dllsDirectory, dllName);

            bool success = MemoryEngine.InjectDll(pid, dllFullPath);
            if (success)
            {
                lblStatus.Text = $"Durum: Başarıyla enjekte edildi -> {dllName} (PID: {pid})";
                MessageBox.Show("DLL başarıyla enjekte edildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Durum: Enjeksiyon başarısız! Yetki yetersiz olabilir.";
                MessageBox.Show("Enjeksiyon başarısız oldu. Yönetici hakları gerekebilir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
