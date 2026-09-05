# OWASP Top 10 Security Lab - Master Dashboard

The **Master Application** (`MasterApp`) serves as an orchestration hub above all 20 individual OWASP Top 10 projects.

---

## Key Features

1. **Vulnerability Tiles Grid**:
   - 10 interactive tiles representing each OWASP Top 10 (2021) category (`A01` through `A10`).
   - Displays category badges, title, summary, and CWE identifiers.
2. **Integrated Iframe Demo Runner**:
   - Clicking any tile launches the **Vulnerable Demo** directly inside an integrated live `<iframe>`.
   - Each tile features a dedicated **"🟢 Fixed Version"** button to load the remediated defense in the iframe.
   - On-demand service lifecycle: automatically spins up the corresponding .NET background service if it is not already running.
3. **Embedded Mode Switcher & Media Tabs**:
   - `🔴 Live Vulnerable Demo (iframe)`: live interactive exploit testing.
   - `🟢 Live Fixed Version (iframe)`: live interactive remediation verification.
   - `🎬 Attack Video`: pre-recorded automated Playwright attack movie.
   - `🛡️ Defense Video`: pre-recorded automated Playwright defense movie.
   - `📖 Technical Details`: comprehensive explanation of the vulnerability and secure fix.
4. **Service Process Manager**:
   - `GET /api/modules`: lists metadata for all 10 modules.
   - `POST /api/service/start`: starts or checks background service for any project.
   - `POST /api/service/stop-all`: stops all background .NET processes spawned by the Master.

---

## How to Run

```bash
cd MasterApp
dotnet run --urls http://localhost:5050
```

Navigate to **`http://localhost:5050`** in your browser.

---

## Video Demonstration

An automated Playwright recording of the Master App in action is available:
- File: [`recordings/owasp-master-app-demo.webm`](./recordings/owasp-master-app-demo.webm)

To re-run the Playwright recording:
```bash
cd MasterApp
npm install playwright
dotnet run --urls http://localhost:5050 &
PID=$!
sleep 4
node record_demo.js
kill $PID
```
