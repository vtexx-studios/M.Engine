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
        private ComboBox cmbMethods = null!;
        private TextBox txtSearch = null!;
        private Button btnRefresh = null!;
        private Button btnInject = null!;
        private Button btnScanKey = null!;
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
            this.Text = "M.Engine - Advanced DLL Injector (Administrator Mode)";
            this.Width = 680;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeComponents();
            LoadProcesses();
            LoadDlls();
        }

        private void InitializeComponents()
        {
            // Search Box
            txtSearch = new TextBox { Location = new Point(12, 12), Size = new Size(380, 23), PlaceholderText = "Search process..." };
            txtSearch.TextChanged += (s, e) => FilterProcesses(txtSearch.Text);
            this.Controls.Add(txtSearch);

            // Process List
            lstProcesses = new ListBox { Location = new Point(12, 42), Size = new Size(380, 260) };
            this.Controls.Add(lstProcesses);

            // DLL Selection Menu
            Label lblDll = new Label { Text = "Target DLL:", Location = new Point(405, 15), Size = new Size(250, 20) };
            this.Controls.Add(lblDll);

            cmbDlls = new ComboBox { Location = new Point(405, 42), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDlls.SelectedIndexChanged += CmbDlls_SelectedIndexChanged;
            this.Controls.Add(cmbDlls);

            // Injection Method Selection
            Label lblMethod = new Label { Text = "Injection Method:", Location = new Point(405, 75), Size = new Size(250, 20) };
            this.Controls.Add(lblMethod);

            cmbMethods = new ComboBox { Location = new Point(405, 98), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethods.Items.Add("Method 1: Standard LoadLibrary");
            cmbMethods.Items.Add("Method 2: Alternative Stub");
            cmbMethods.SelectedIndex = 0;
            this.Controls.Add(cmbMethods);

            // Hotkey Scanner Button
            btnScanKey = new Button { Text = "Scan DLL Menu Hotkey", Location = new Point(405, 132), Size = new Size(250, 30) };
            btnScanKey.Click += BtnScanKey_Click;
            this.Controls.Add(btnScanKey);

            // Refresh Button
            btnRefresh = new Button { Text = "Refresh Lists", Location = new Point(405, 172), Size = new Size(250, 35) };
            btnRefresh.Click += (s, e) => { LoadProcesses(); LoadDlls(); Log("Lists refreshed successfully."); };
            this.Controls.Add(btnRefresh);

            // Inject Button
            btnInject = new Button { Text = "INJECT DLL", Location = new Point(405, 215), Size = new Size(250, 45), BackColor = Color.ForestGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnInject.Click += BtnInject_Click;
            this.Controls.Add(btnInject);

            // Log Panel
            Label lblLog = new Label { Text = "Execution Logs:", Location = new Point(12, 310), Size = new Size(380, 20) };
            this.Controls.Add(lblLog);

            txtLogs = new TextBox { Location = new Point(12, 335), Size = new Size(643, 125), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.Black, ForeColor = Color.Lime };
            this.Controls.Add(txtLogs);

            // Status Bar
            lblStatus = new Label { Text = "Status: Ready (Running as Administrator)", Location = new Point(12, 470), Size = new Size(643, 25), ForeColor = Color.DarkSlateGray };
            this.Controls.Add(lblStatus);
        }

        private void LoadProcesses()
        {
            allProcesses.Clear();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(p.MainWindowTitle) || p.WorkingSet64 > 10000000)
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

        private void CmbDlls_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbDlls.SelectedItem != null)
            {
                string dllName = cmbDlls.SelectedItem.ToString() ?? "";
                string dllPath = Path.Combine(dllsDirectory, dllName);
                string detectedKey = MemoryEngine.ScanDllForMenuKey(dllPath);
                Log($"Selected DLL: {dllName} | Estimated Menu Hotkey: {detectedKey}");
            }
        }

        private void BtnScanKey_Click(object? sender, EventArgs e)
        {
            if (cmbDlls.SelectedItem == null)
            {
                MessageBox.Show("Please select a DLL first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dllName = cmbDlls.SelectedItem.ToString() ?? "";
            string dllPath = Path.Combine(dllsDirectory, dllName);
            string keyResult = MemoryEngine.ScanDllForMenuKey(dllPath);

            Log($"[HOTKEY SCANNER] Result for {dllName}: {keyResult}");
            MessageBox.Show($"Detected Menu Hotkey / Signature:\n{keyResult}", "Hotkey Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnInject_Click(object? sender, EventArgs e)
        {
            if (lstProcesses.SelectedItem is not ProcessInfo selectedProc)
            {
                MessageBox.Show("Please select a target process from the list!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDlls.SelectedItem == null)
            {
                MessageBox.Show("No DLL found in the dlls/ directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dllName = cmbDlls.SelectedItem.ToString() ?? "";
            string dllFullPath = Path.Combine(dllsDirectory, dllName);
            int methodIndex = cmbMethods.SelectedIndex;

            Log($"Attempting injection -> Process: {selectedProc.Name} (PID: {selectedProc.Id}), DLL: {dllName}, Method: {methodIndex + 1}");

            bool success = false;
            string errorDetails = string.Empty;

            if (methodIndex == 0)
            {
                success = MemoryEngine.InjectStandard(selectedProc.Id, dllFullPath, out errorDetails);
            }
            else
            {
                success = MemoryEngine.InjectAlternative(selectedProc.Id, dllFullPath, out errorDetails);
            }

            if (success)
            {
                lblStatus.Text = $"Status: Successfully injected -> {dllName}";
                Log($"[SUCCESS] {dllName} successfully injected into PID {selectedProc.Id}.");
                MessageBox.Show("DLL injected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Status: Injection Failed!";
                Log($"[ERROR] Injection failed. Details: {errorDetails}");
                MessageBox.Show($"Injection failed!\nDetails: {errorDetails}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
