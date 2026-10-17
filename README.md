# M.Engine 🚀

**M.Engine** is a lightweight **DLL Injector** tool developed using C# (`.NET 8.0`) and Windows Forms, featuring modern architecture detection (`x86` / `x64`), advanced process management, and detailed logging capabilities.

---

## 📂 Project Architecture & File Structure

The project's modular source code and directory layout are structured as follows:

M.Engine/
│
├── .github/workflows/
│   └── build.yml          # Automated CI/CD build pipeline
├── bin/                   # Compiled binary output directory
├── src/                   # Source code files
│   ├── MainForm.cs        # UI, search filters, and application logic
│   ├── MemoryEngine.cs    # P/Invoke-based memory and injection APIs
│   ├── ProcessManager.cs  # Process management and x86/x64 arch detection
│   └── Program.cs         # Main application entry point
├── dlls/                  # Directory where target DLL files are stored
├── ProcessEngine.csproj   # Project and .NET 8.0 configuration file
├── app.manifest           # Administrator privileges manifest (requireAdministrator)
├── mengine.cmd            # Quick run wrapper command script
├── install.bat            # Automated setup wizard
└── README.md              # Project documentation

---

## ✨ Key Features

* **Architecture Compatibility Check:** Automatically detects whether the target process is 32-bit (`x86`) or 64-bit (`x64`) to prevent cross-architecture mismatch errors.
* **Live Process Filtering:** Dynamic search box allowing real-time filtering through hundreds of active running processes.
* **Advanced Error Logging:** Real-time log panel displaying operation statuses and Windows API (`Marshal.GetLastWin32Error`) error codes in a dark/green console interface.
* **Administrator Privileges:** Built-in application manifest support that automatically requests administrator elevation required for process memory access.

---

## 🛠️ Installation & Building

To build the project locally, you must have the **.NET 8.0 SDK** installed on your system.

1. Clone or download the repository to your machine.
2. Open a terminal (CMD or PowerShell) in the project root directory.
3. Build the project in Release mode using the following command:
   `dotnet build -c Release`
4. Alternatively, run the **`install.bat`** script for automated setup and file association.

---

## 🚀 Usage Guide

1. Ensure a directory named **`dlls`** exists in the compiled output folder (it will be created automatically if missing).
2. Place the target DLL file inside the `dlls/` folder.
3. Right-click **M.Engine.exe** and select **Run as Administrator**.
4. Use the search bar at the top to locate your target process and select it from the list.
5. Select the desired DLL from the drop-down menu on the right panel.
6. Click the **"DLL ENJEKTE ET"** (Inject DLL) button. Monitor the log panel for status updates and error details.
