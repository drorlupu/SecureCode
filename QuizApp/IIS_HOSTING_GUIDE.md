# 🌐 QuizApp IIS Hosting & Deployment Guide

This guide details how to deploy and host the **OWASP Top 10 (2025) Interactive Security Quiz** in **Microsoft Internet Information Services (IIS)** on Windows Server 2019/2022/2025 or Windows 10/11.

---

## 📋 1. Server Prerequisites

Before configuring IIS, install the required runtime and Windows features:

### A. Install .NET 10 Hosting Bundle
The **.NET Hosting Bundle** installs the .NET Runtime and the **ASP.NET Core Module (ANCM v2)** into IIS.
1. Download the **.NET 10.0 Hosting Bundle** from Microsoft's official .NET download page.
2. Run the installer on the IIS server.
3. Restart IIS via command prompt (Admin):
   ```cmd
   net stop was /y
   net start w3svc
   ```

### B. Enable the IIS WebSocket Feature (Crucial for SignalR)
SignalR relies on WebSockets for real-time, low-latency communication during the quiz.

- **Via PowerShell (Run as Administrator)**:
  ```powershell
  # For Windows Server:
  Install-WindowsFeature -Name Web-WebSockets

  # For Windows 10 / 11:
  Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets
  ```
- **Via GUI (Server Manager)**:
  - Open **Server Manager** $\rightarrow$ **Add Roles and Features**.
  - Navigate to **Server Roles** $\rightarrow$ **Web Server (IIS)** $\rightarrow$ **Web Server** $\rightarrow$ **Application Development**.
  - Check **WebSocket Protocol** and complete the wizard.

---

## 📦 2. Publish the Application

Run the publish script or standard `dotnet publish` command:

### Option A: Using the PowerShell Script
```powershell
.\QuizApp\publish-for-iis.ps1
```

### Option B: Manual Command Line
```cmd
dotnet publish QuizApp/QuizApp.csproj -c Release -o C:\inetpub\wwwroot\QuizApp
```

The publish directory will contain:
- `QuizApp.dll` (Compiled application assembly)
- `web.config` (Pre-configured for ASP.NET Core In-Process hosting & WebSockets)
- `wwwroot/` (Static assets, HTML, JavaScript, SignalR client library)
- `appsettings.json`

---

## ⚙️ 3. IIS Configuration

### Step 1: Create a Dedicated Application Pool
1. Open **Internet Information Services (IIS) Manager** (`inetmgr`).
2. In the left **Connections** pane, click **Application Pools**.
3. In the right **Actions** pane, click **Add Application Pool...**:
   - **Name:** `QuizAppPool`
   - **.NET CLR version:** **No Managed Code** *(Essential: ASP.NET Core runs out-of-band via ANCM)*
   - **Managed pipeline mode:** **Integrated**
4. Click **OK**.
5. Select `QuizAppPool`, click **Advanced Settings...**:
   - **Enable 32-Bit Applications:** `False`
   - **Start Mode:** `AlwaysRunning` *(Optional, reduces cold-start latency)*
   - **Idle Time-out (minutes):** Set to `0` or higher to prevent premature shutdown during a workshop.

---

### Step 2: Choose Your Deployment Topology

#### Scenario A: Deploy as a New Standalone Website
Use this if you have a dedicated port (e.g. `http://server:5055`) or a host header/DNS record (e.g. `http://quiz.internal.corp/`).
1. Right-click **Sites** $\rightarrow$ **Add Website...**.
2. Set:
   - **Site name:** `QuizApp`
   - **Application pool:** `QuizAppPool`
   - **Physical path:** `C:\inetpub\wwwroot\QuizApp` (your published folder)
   - **Binding:** `http`, Port `80` (or `5055`), Host name: (leave blank or specify domain).
3. Click **OK**.

#### Scenario B: Deploy as a Sub-Application (e.g. `/quiz`)
Use this if you want the quiz hosted under your existing Default Web Site (e.g. `http://server/quiz`).
1. Expand **Sites** $\rightarrow$ **Default Web Site**.
2. Right-click **Default Web Site** $\rightarrow$ **Add Application...**:
   - **Alias:** `quiz`
   - **Application pool:** `QuizAppPool`
   - **Physical path:** `C:\inetpub\wwwroot\QuizApp`
3. Click **OK**.
4. Access via: `http://localhost/quiz/`.  
   *(The client JS dynamically senses the `/quiz` prefix and routes SignalR `/quiz/quizhub` without any configuration changes).*

---

### Step 3: Set NTFS File Permissions
Ensure the IIS application pool identity can read and execute files in the folder:
1. Open Windows Explorer and navigate to your publish folder (`C:\inetpub\wwwroot\QuizApp`).
2. Right-click the folder $\rightarrow$ **Properties** $\rightarrow$ **Security** tab $\rightarrow$ **Edit...**.
3. Click **Add...**, enter `IIS_IUSRS`, and click **OK**.
4. Ensure **Read & execute**, **List folder contents**, and **Read** are checked.
5. *(Optional)* If you enable file-based stdout logging in `web.config`, create a `logs` subfolder and grant `IIS_IUSRS` **Modify** permissions on that `logs` folder.

---

## 🔍 4. Verification & Diagnostics

1. **Open Browser:** Navigate to `http://your-server/` (or `http://your-server/quiz/`).
2. **Open DevTools (`F12` $\rightarrow$ Console):**
   - You should see:
     ```
     [SignalR] Connected to hub: /quizhub (or /quiz/quizhub)
     ```
3. **Open DevTools (`F12` $\rightarrow$ Network $\rightarrow$ WS filter):**
   - Find the request named `quizhub`.
   - The status should be **`101 Switching Protocols`**, confirming active WebSockets transport.
4. **Test Health Endpoint:**
   - Navigate to `http://your-server/api/quiz/health`
   - Expected response:
     ```json
     {"status":"Healthy","app":"KahootCloneSecurityQuiz","standard":"OWASP Top 10:2025"}
     ```

---

## 🛠️ 5. Troubleshooting Common IIS Issues

| Issue / Error | Cause | Resolution |
| :--- | :--- | :--- |
| **HTTP Error 500.19 – Internal Server Error** | Missing ASP.NET Core Module (ANCM) in IIS. | Install the **.NET 10 Hosting Bundle** and restart IIS (`net stop was /y && net start w3svc`). |
| **HTTP Error 500.30 – ASP.NET Core In-Process Failure** | Unhandled startup exception or missing .NET 10 runtime. | In `web.config`, temporarily set `stdoutLogEnabled="true"`, recreate the issue, and inspect `.\logs\stdout_*.log`. |
| **HTTP Error 502.5 – Process Failure** | In-process crash or 32-bit/64-bit mismatch. | Check Event Viewer (`Application` log) for events from source `IIS AspNetCore Module V2`. Ensure AppPool is 64-bit (`Enable 32-Bit Applications = False`). |
| **SignalR falls back to Long Polling / SSE** | WebSocket Protocol feature not enabled in IIS. | Run `Install-WindowsFeature -Name Web-WebSockets` in PowerShell (Admin) and restart IIS. |
| **SignalR 404 when hosted in subfolder** | Static hub path routing issue. | Fixed in our client: `resolveHubUrl()` dynamically reads `window.location.pathname` to construct `/subfolder/quizhub`. |
