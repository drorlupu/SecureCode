# OWASP Top 10 (2021) .NET Benchmark: Vulnerable vs. Remediated Suite

This repository contains a full set of 20 .NET 10 Web API projects demonstrating the **OWASP Top 10 (2021)**:
- **`01` through `10`**: Realistic vulnerable implementations for benchmarking and static security code review.
- **`Fixed/` (`01` through `10`)**: Remediated, production-grade secure implementations resolving every identified vulnerability.
- **`MasterApp` (Master Orchestrator)**: Unified web application with interactive tiles for each vulnerability, allowing users to invoke vulnerable demos and fixed counterparts inside an integrated live iframe or switch to video walkthroughs.
- **Interactive UI & Playwright Video Demos**: Every project (both vulnerable and fixed) contains an interactive web UI (`wwwroot/index.html`) and an automated Playwright video recording script (`record_demo.js`) producing high-resolution `.webm` demonstration movies.

---

## 🌟 Master Application Quickstart

To launch the unified Master Dashboard hosting all 10 vulnerability tiles:
```bash
cd MasterApp
dotnet run --urls http://localhost:5050
```
Open **`http://localhost:5050`** in your browser.
- **Click any tile** (or click **"🔴 Vulnerable Demo"**): boots and loads the vulnerable version in the integrated iframe!
- **Click "🟢 Fixed Version"**: boots and loads the remediated defense version in the integrated iframe!
- **Click "🎬 Watch Video Walkthrough"**: plays the automated Playwright movie of the attack or defense!
- Demonstration video: [`MasterApp/recordings/owasp-master-app-demo.webm`](./MasterApp/recordings/owasp-master-app-demo.webm)

---

## Video Demonstrations & Matrix

All 20 projects have pre-recorded automated Playwright videos demonstrating the vulnerability exploit and the fixed defense:

| OWASP Top 10 Category | Vulnerable Project & Video | Remediated Project & Video | Key Remediation |
|:---|:---|:---|:---|
| **A01: Broken Access Control** | [`01-BrokenAccessControl/`](./01-BrokenAccessControl/)<br>🎬 [owasp-a01-broken-access-control.webm](./01-BrokenAccessControl/recordings/owasp-a01-broken-access-control.webm) | [`Fixed/01-BrokenAccessControl/`](./Fixed/01-BrokenAccessControl/)<br>🎬 [owasp-a01-broken-access-control-fixed.webm](./Fixed/01-BrokenAccessControl/recordings/owasp-a01-broken-access-control-fixed.webm) | Resource ownership verification (`OwnerId == currentUserId`), `.RequireAuthorization()`, and role mass-assignment prevention via DTOs. |
| **A02: Cryptographic Failures** | [`02-CryptographicFailures/`](./02-CryptographicFailures/)<br>🎬 [owasp-a02-cryptographic-failures.webm](./02-CryptographicFailures/recordings/owasp-a02-cryptographic-failures.webm) | [`Fixed/02-CryptographicFailures/`](./Fixed/02-CryptographicFailures/)<br>🎬 [owasp-a02-cryptographic-failures-fixed.webm](./Fixed/02-CryptographicFailures/recordings/owasp-a02-cryptographic-failures-fixed.webm) | Upgraded broken MD5 to salted PBKDF2 (`PasswordHasher<T>`); replaced ECB with authenticated AES-256-GCM (`AesGcm`). |
| **A03: Injection** | [`03-Injection/`](./03-Injection/)<br>🎬 [owasp-a03-injection.webm](./03-Injection/recordings/owasp-a03-injection.webm) | [`Fixed/03-Injection/`](./Fixed/03-Injection/)<br>🎬 [owasp-a03-injection-fixed.webm](./Fixed/03-Injection/recordings/owasp-a03-injection-fixed.webm) | Parameterized SQL commands (`@query`) in SQLite and replaced command execution with native .NET `Ping` API. |
| **A04: Insecure Design** | [`04-InsecureDesign/`](./04-InsecureDesign/)<br>🎬 [owasp-a04-insecure-design.webm](./04-InsecureDesign/recordings/owasp-a04-insecure-design.webm) | [`Fixed/04-InsecureDesign/`](./Fixed/04-InsecureDesign/)<br>🎬 [owasp-a04-insecure-design-fixed.webm](./Fixed/04-InsecureDesign/recordings/owasp-a04-insecure-design-fixed.webm) | Single coupon redemption constraint, server-authoritative pricing, and cryptographically random password reset tokens. |
| **A05: Security Misconfiguration** | [`05-SecurityMisconfiguration/`](./05-SecurityMisconfiguration/)<br>🎬 [owasp-a05-security-misconfiguration.webm](./05-SecurityMisconfiguration/recordings/owasp-a05-security-misconfiguration.webm) | [`Fixed/05-SecurityMisconfiguration/`](./Fixed/05-SecurityMisconfiguration/)<br>🎬 [owasp-a05-security-misconfiguration-fixed.webm](./Fixed/05-SecurityMisconfiguration/recordings/owasp-a05-security-misconfiguration-fixed.webm) | Environment-guarded error pages (`app.UseExceptionHandler()`), strict CORS origin whitelisting, and CSP security headers. |
| **A06: Vulnerable Components** | [`06-VulnerableAndOutdatedComponents/`](./06-VulnerableAndOutdatedComponents/)<br>🎬 [owasp-a06-vulnerable-components.webm](./06-VulnerableAndOutdatedComponents/recordings/owasp-a06-vulnerable-components.webm) | [`Fixed/06-VulnerableAndOutdatedComponents/`](./Fixed/06-VulnerableAndOutdatedComponents/)<br>🎬 [owasp-a06-vulnerable-components-fixed.webm](./Fixed/06-VulnerableAndOutdatedComponents/recordings/owasp-a06-vulnerable-components-fixed.webm) | Replaced vulnerable `Newtonsoft.Json 12.0.1` with modern `System.Text.Json` and enabled `<NuGetAudit>true</NuGetAudit>`. |
| **A07: Identification & Auth Failures** | [`07-AuthFailures/`](./07-AuthFailures/)<br>🎬 [owasp-a07-auth-failures.webm](./07-AuthFailures/recordings/owasp-a07-auth-failures.webm) | [`Fixed/07-AuthFailures/`](./Fixed/07-AuthFailures/)<br>🎬 [owasp-a07-auth-failures-fixed.webm](./Fixed/07-AuthFailures/recordings/owasp-a07-auth-failures-fixed.webm) | Implemented rate limiting (`Microsoft.AspNetCore.RateLimiting`), account lockout, and strict password complexity rules. |
| **A08: Software & Data Integrity** | [`08-SoftwareAndDataIntegrityFailures/`](./08-SoftwareAndDataIntegrityFailures/)<br>🎬 [owasp-a08-data-integrity.webm](./08-SoftwareAndDataIntegrityFailures/recordings/owasp-a08-data-integrity.webm) | [`Fixed/08-SoftwareAndDataIntegrityFailures/`](./Fixed/08-SoftwareAndDataIntegrityFailures/)<br>🎬 [owasp-a08-data-integrity-fixed.webm](./Fixed/08-SoftwareAndDataIntegrityFailures/recordings/owasp-a08-data-integrity-fixed.webm) | Replaced insecure binary deserialization with typed JSON, and added HMAC-SHA256 signature verification on external payloads. |
| **A09: Logging & Monitoring Failures** | [`09-LoggingAndMonitoringFailures/`](./09-LoggingAndMonitoringFailures/)<br>🎬 [owasp-a09-logging-failures.webm](./09-LoggingAndMonitoringFailures/recordings/owasp-a09-logging-failures.webm) | [`Fixed/09-LoggingAndMonitoringFailures/`](./Fixed/09-LoggingAndMonitoringFailures/)<br>🎬 [owasp-a09-logging-failures-fixed.webm](./Fixed/09-LoggingAndMonitoringFailures/recordings/owasp-a09-logging-failures-fixed.webm) | Sanitized credentials from logs, added structured audit trails, and correlation IDs with SIEM alerts on suspicious errors. |
| **A10: Server-Side Request Forgery** | [`10-ServerSideRequestForgery/`](./10-ServerSideRequestForgery/)<br>🎬 [owasp-a10-ssrf.webm](./10-ServerSideRequestForgery/recordings/owasp-a10-ssrf.webm) | [`Fixed/10-ServerSideRequestForgery/`](./Fixed/10-ServerSideRequestForgery/)<br>🎬 [owasp-a10-ssrf-fixed.webm](./Fixed/10-ServerSideRequestForgery/recordings/owasp-a10-ssrf-fixed.webm) | Enforces URL scheme validation, resolves host IP, and strictly blocks loopback (`127.0.0.1`), RFC 1918 private subnets, and cloud metadata (`169.254.169.254`). |

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
