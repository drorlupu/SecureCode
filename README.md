# OWASP Top 10 (2025) .NET Benchmark: Vulnerable vs. Remediated Suite

This repository contains a full set of 20 .NET 10 Web API projects demonstrating the **OWASP Top 10:2025** edition:
- **`01` through `10`**: Realistic vulnerable implementations for benchmarking and static security code review.
- **`Fixed/` (`01` through `10`)**: Remediated, production-grade secure implementations resolving every identified vulnerability.
- **`MasterApp` (Master Orchestrator)**: Unified web application with interactive tiles for each OWASP Top 10 (2025) category, allowing users to launch vulnerable demos and fixed counterparts inside an integrated live iframe or switch to video walkthroughs.
- **`QuizApp` (Interactive Multiplayer Security Kahoot)**: Real-time SignalR quiz application featuring millisecond-accurate speed bonuses, host dashboard controls, and player screen with shape/color cards.
- **Interactive UI & Playwright Video Demos**: Every project (both vulnerable and fixed) contains an interactive web UI (`wwwroot/index.html`) and an automated Playwright video recording script (`record_demo.js`) producing high-resolution `.webm` demonstration movies.

---

## 🌟 Master Application & Quiz Quickstart

### Master Dashboard
To launch the unified Master Dashboard hosting all 10 OWASP Top 10 (2025) vulnerability tiles:
```bash
cd MasterApp
dotnet run --urls http://localhost:5050
```
Open **`http://localhost:5050`** in your browser.
- **Click any tile** (or click **"🔴 Vulnerable Demo"**): boots and loads the vulnerable version in the integrated iframe!
- **Click "🟢 Fixed Version"**: boots and loads the remediated defense version in the integrated iframe!
- **Click "🎬 Watch Video Walkthrough"**: plays the automated Playwright movie of the attack or defense!
- **Click "🎯 Interactive Security Quiz"**: opens the multiplayer Kahoot-style quiz!

### Multiplayer Quiz App
To launch the SignalR live quiz directly:
```bash
cd QuizApp
dotnet run --urls http://localhost:5055
```
Open **`http://localhost:5055`** in your browser.

---

## OWASP Top 10:2025 Demonstrations & Matrix

The benchmark aligns with the 2025 standard:

| 2025 Category | Vulnerable Project & Video | Remediated Project & Video | Key Remediation & 2025 Context |
|:---|:---|:---|:---|
| **A01:2025 – Broken Access Control** | [`01-BrokenAccessControl/`](./01-BrokenAccessControl/)<br>🎬 [owasp-a01-broken-access-control.webm](./01-BrokenAccessControl/recordings/owasp-a01-broken-access-control.webm) | [`Fixed/01-BrokenAccessControl/`](./Fixed/01-BrokenAccessControl/)<br>🎬 [owasp-a01-broken-access-control-fixed.webm](./Fixed/01-BrokenAccessControl/recordings/owasp-a01-broken-access-control-fixed.webm) | Consolidates SSRF & BOLA/IDOR. Enforces resource ownership (`OwnerId == currentUserId`), IP/subnet SSRF blocklists, and DTO role protection. |
| **A02:2025 – Security Misconfiguration** | [`02-SecurityMisconfiguration/`](./02-SecurityMisconfiguration/)<br>🎬 [owasp-a02-security-misconfiguration.webm](./02-SecurityMisconfiguration/recordings/owasp-a02-security-misconfiguration.webm) | [`Fixed/02-SecurityMisconfiguration/`](./Fixed/02-SecurityMisconfiguration/)<br>🎬 [owasp-a02-security-misconfiguration-fixed.webm](./Fixed/02-SecurityMisconfiguration/recordings/owasp-a02-security-misconfiguration-fixed.webm) | Rose to #2 in 2025. Standardized RFC 7807 ProblemDetails, strict CORS origin whitelisting, and CSP security headers. |
| **A03:2025 – Software Supply Chain Failures** | [`03-SoftwareSupplyChainFailures/`](./03-SoftwareSupplyChainFailures/)<br>🎬 [owasp-a03-software-supply-chain-failures.webm](./03-SoftwareSupplyChainFailures/recordings/owasp-a03-software-supply-chain-failures.webm) | [`Fixed/03-SoftwareSupplyChainFailures/`](./Fixed/03-SoftwareSupplyChainFailures/)<br>🎬 [owasp-a03-software-supply-chain-failures-fixed.webm](./Fixed/03-SoftwareSupplyChainFailures/recordings/owasp-a03-software-supply-chain-failures-fixed.webm) | Expands beyond vulnerable components to pipeline risks. Migrated to modern `System.Text.Json` and enabled `<NuGetAudit>true</NuGetAudit>`. |
| **A04:2025 – Cryptographic Failures** | [`04-CryptographicFailures/`](./04-CryptographicFailures/)<br>🎬 [owasp-a04-cryptographic-failures.webm](./04-CryptographicFailures/recordings/owasp-a04-cryptographic-failures.webm) | [`Fixed/04-CryptographicFailures/`](./Fixed/04-CryptographicFailures/)<br>🎬 [owasp-a04-cryptographic-failures-fixed.webm](./Fixed/04-CryptographicFailures/recordings/owasp-a04-cryptographic-failures-fixed.webm) | Upgraded broken MD5 to salted PBKDF2 (`PasswordHasher<T>`); replaced ECB with authenticated AES-256-GCM (`AesGcm`). |
| **A05:2025 – Injection** | [`05-Injection/`](./05-Injection/)<br>🎬 [owasp-a05-injection.webm](./05-Injection/recordings/owasp-a05-injection.webm) | [`Fixed/05-Injection/`](./Fixed/05-Injection/)<br>🎬 [owasp-a05-injection-fixed.webm](./Fixed/05-Injection/recordings/owasp-a05-injection-fixed.webm) | Parameterized SQL commands (`@query`) in SQLite and replaced raw shell commands with native .NET `Ping` API. |
| **A06:2025 – Insecure Design** | [`06-InsecureDesign/`](./06-InsecureDesign/)<br>🎬 [owasp-a06-insecure-design.webm](./06-InsecureDesign/recordings/owasp-a06-insecure-design.webm) | [`Fixed/06-InsecureDesign/`](./Fixed/06-InsecureDesign/)<br>🎬 [owasp-a06-insecure-design-fixed.webm](./Fixed/06-InsecureDesign/recordings/owasp-a06-insecure-design-fixed.webm) | Single-use coupon redemption constraint, server-authoritative pricing, and cryptographically random reset tokens. |
| **A07:2025 – Authentication Failures** | [`07-AuthFailures/`](./07-AuthFailures/)<br>🎬 [owasp-a07-auth-failures.webm](./07-AuthFailures/recordings/owasp-a07-auth-failures.webm) | [`Fixed/07-AuthFailures/`](./Fixed/07-AuthFailures/)<br>🎬 [owasp-a07-auth-failures-fixed.webm](./Fixed/07-AuthFailures/recordings/owasp-a07-auth-failures-fixed.webm) | Implemented ASP.NET Core RateLimiting, automated account lockout, and robust password complexity requirements. |
| **A08:2025 – Software & Data Integrity Failures** | [`08-SoftwareAndDataIntegrityFailures/`](./08-SoftwareAndDataIntegrityFailures/)<br>🎬 [owasp-a08-data-integrity.webm](./08-SoftwareAndDataIntegrityFailures/recordings/owasp-a08-data-integrity.webm) | [`Fixed/08-SoftwareAndDataIntegrityFailures/`](./Fixed/08-SoftwareAndDataIntegrityFailures/)<br>🎬 [owasp-a08-data-integrity-fixed.webm](./Fixed/08-SoftwareAndDataIntegrityFailures/recordings/owasp-a08-data-integrity-fixed.webm) | Replaced insecure binary deserialization with typed JSON, and added HMAC-SHA256 signature verification on external payloads. |
| **A09:2025 – Security Logging & Alerting Failures** | [`09-LoggingAndAlertingFailures/`](./09-LoggingAndAlertingFailures/)<br>🎬 [owasp-a09-logging-and-alerting-failures.webm](./09-LoggingAndAlertingFailures/recordings/owasp-a09-logging-and-alerting-failures.webm) | [`Fixed/09-LoggingAndAlertingFailures/`](./Fixed/09-LoggingAndAlertingFailures/)<br>🎬 [owasp-a09-logging-and-alerting-failures-fixed.webm](./Fixed/09-LoggingAndAlertingFailures/recordings/owasp-a09-logging-and-alerting-failures-fixed.webm) | Sanitized credentials from telemetry, added structured audit trails, and correlation IDs with active SIEM alerting. |
| **A10:2025 – Mishandling of Exceptional Conditions** | [`10-MishandlingOfExceptionalConditions/`](./10-MishandlingOfExceptionalConditions/)<br>🎬 [owasp-a10-exceptional-conditions.webm](./10-MishandlingOfExceptionalConditions/recordings/owasp-a10-exceptional-conditions.webm) | [`Fixed/10-MishandlingOfExceptionalConditions/`](./Fixed/10-MishandlingOfExceptionalConditions/)<br>🎬 [owasp-a10-exceptional-conditions-fixed.webm](./Fixed/10-MishandlingOfExceptionalConditions/recordings/owasp-a10-exceptional-conditions-fixed.webm) | Brand new 2025 category: Demonstrates Fail-Closed exception boundaries (preventing bypasses when microservices fail) alongside IP-restricted SSRF defenses. |


---

## Running and Recording Demonstrations

### 1. Build Entire Solution
```bash
dotnet build SecureCode.slnx
```

### 2. Interactive Web UI
Every project serves an interactive HTML UI at the root path (`/`). To explore:
```bash
cd 01-BrokenAccessControl # or Fixed/01-BrokenAccessControl
dotnet run
# Open http://localhost:5000 in your browser
```

### 3. Re-record Playwright Videos
To record the automated demonstration movie for any project:
```bash
cd 01-BrokenAccessControl
npm install playwright
dotnet run --urls http://localhost:5005 &
PID=$!
sleep 3
node record_demo.js
kill $PID
```
The resulting video will be saved in the project's `recordings/` directory as a `.webm` video.

### 4. Run Security Code Review Skill
Use the workspace skill located at [`.agents/skills/security-code-review/SKILL.md`](./.agents/skills/security-code-review/SKILL.md) to audit any project against the OWASP Top 10.
