---
marp: true
theme: default
class: invert
paginate: true
header: "🛡️ Secure Coding Workshop | C# & TypeScript"
footer: "Confidential - Internal Engineering Workshop"
style: |
  /* Global Section Styling */
  section {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
    background: linear-gradient(135deg, #0b0f19 0%, #111827 100%);
    color: #f1f5f9;
    font-size: 23px;
    line-height: 1.45;
    padding: 38px 55px;
  }

  /* Headings */
  h1 {
    color: #38bdf8;
    font-size: 1.8em;
    font-weight: 700;
    margin-bottom: 0.3em;
    border-bottom: none;
  }
  h2 {
    color: #38bdf8;
    font-size: 1.3em;
    font-weight: 600;
    border-bottom: 2px solid #334155;
    padding-bottom: 6px;
    margin-top: 0;
    margin-bottom: 0.45em;
  }
  h3 {
    color: #93c5fd;
    font-size: 1.02em;
    font-weight: 600;
    margin-top: 0.3em;
    margin-bottom: 0.25em;
  }

  /* Header & Footer */
  header {
    color: #64748b;
    font-size: 0.62em;
    font-weight: 500;
    top: 18px;
    left: 55px;
  }
  footer {
    color: #64748b;
    font-size: 0.62em;
    font-weight: 500;
    bottom: 18px;
    left: 55px;
  }
  section::after {
    color: #64748b;
    font-size: 0.62em;
    bottom: 18px;
    right: 55px;
  }

  /* Paragraphs & Lists */
  p, li {
    color: #e2e8f0;
    font-size: 0.95em;
  }
  ul, ol {
    margin-top: 6px;
    margin-bottom: 8px;
    padding-left: 1.5em;
  }
  li {
    margin-bottom: 6px;
  }

  /* Links */
  a {
    color: #38bdf8;
    text-decoration: underline;
    text-underline-offset: 3px;
  }
  a:hover {
    color: #7dd3fc;
  }

  /* Code & Pre Blocks */
  code {
    font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
    background-color: #1e293b;
    color: #38bdf8;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.86em;
  }
  pre {
    background-color: #0b1120;
    border: 1px solid #1e293b;
    border-radius: 8px;
    padding: 10px 14px;
    margin: 6px 0;
    box-shadow: 0 4px 12px rgba(0,0,0,0.3);
  }
  pre code {
    background: transparent;
    color: #e2e8f0;
    font-size: 0.68em;
    line-height: 1.32;
    padding: 0;
  }

  /* Tables */
  table {
    width: 100%;
    border-collapse: collapse;
    margin: 8px 0;
    font-size: 0.72em;
    background-color: #131d31;
    border: 1px solid #334155;
    border-radius: 8px;
    overflow: hidden;
  }
  th {
    background-color: #1e293b !important;
    color: #38bdf8 !important;
    font-weight: 600;
    text-align: left;
    padding: 7px 10px;
    border-bottom: 2px solid #334155;
  }
  td {
    padding: 6px 10px;
    border-bottom: 1px solid #1e293b;
    color: #cbd5e1 !important;
    background-color: transparent !important;
  }
  tr:nth-child(2n) {
    background-color: #172236 !important;
  }
  tr:nth-child(2n) td {
    background-color: #172236 !important;
  }
  tr:nth-child(2n+1) {
    background-color: #0f1828 !important;
  }
  tr:nth-child(2n+1) td {
    background-color: #0f1828 !important;
  }

  /* Blockquotes */
  blockquote {
    background-color: #1e293b;
    border-left: 4px solid #38bdf8;
    border-radius: 0 8px 8px 0;
    padding: 8px 14px;
    margin: 8px 0;
    color: #cbd5e1;
    font-size: 0.85em;
  }

  /* Lead Slide Class */
  section.lead {
    text-align: center;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    background: radial-gradient(circle at center, #1e293b 0%, #0b0f19 100%);
  }
  section.lead h1 {
    font-size: 2.1em;
    color: #38bdf8;
    border-bottom: none;
    margin-bottom: 0.2em;
  }
  section.lead h3 {
    font-size: 1.15em;
    color: #94a3b8;
    font-weight: 400;
  }

  .donts {
    color: #f87171;
    font-weight: 700;
  }

  /* Kahoot styling */
  section.kahoot {
    background: linear-gradient(135deg, #1e1b4b 0%, #0b0f19 100%);
  }
  .kahoot-card {
    background-color: #1e1b4b;
    border: 2px dashed #818cf8;
    border-radius: 12px;
    padding: 24px 20px;
    margin-top: 8px;
    text-align: center;
  }
---

# 🛡️ Secure Coding in Practice
### High-Impact Engineering Security Overview

**Format:** Focused Workshop + Culture + Kahoot + Live Demos  
**Target Audience:** Software Engineers, Tech Leads, QA  
**Live Orchestrator:** [OWASP Top 10 Master Dashboard](http://localhost:5050)

---

## 📌 Focused Agenda

1. **Why Secure Code Matters** – Realities of engineering breaches & Shift-Left economics
2. **SDLC & Security Culture** – Integrating security into daily engineering practice
3. **🎮 Kahoot Challenge** – Interactive Team Security Quiz
4. **🚀 Master Application Demo** – Live OWASP Top 10 Vulnerable vs. Remediated Suite
5. **📚 OWASP Top 10 Summary** – Core risk areas & primary defensive controls

---

<!-- _class: lead -->
# 1️⃣ Why Secure Code Matters
### Beyond "Compliance": Security is an Engineering Quality Property

---

## 💥 Small Bugs, Huge Blast Radius

- Security is **not** a feature built at the end; it is a fundamental code quality attribute (like reliability or performance).
- Seemingly trivial bugs cause catastrophic breaches:
  - Missing authorization check on an API endpoint $\rightarrow$ <span class="donts">Mass Data Exfiltration</span>
  - Unsanitized dynamic string query $\rightarrow$ <span class="donts">Database Takeover (SQLi)</span>
  - Hardcoded secret key in client bundle or repository $\rightarrow$ <span class="donts">Account & System Impersonation</span>
- **The Shift Left Advantage:**  
  Fixing a vulnerability in the IDE / Code Review phase costs **$100\times$ less** than resolving an active incident in Production.

---

<!-- _class: lead -->
# 2️⃣ SDLC & Security Culture
### Integrating Security into Everyday Engineering

---

## 🔄 The Integrated Secure SDLC Workflow

```
1. DESIGN           2. CODE & BUILD         3. TEST & MERGE        4. OPERATE
Threat Modeling    -> IDE Linter / SAST    -> Mend SCA Scan       -> Continuous Monitoring
Sessions           (SonarLint / Snyk)      Peer Code Review       (WAF, SIEM, Alerting)
```

### Making Security Habitual:
- **Threat Model** all new epics, architectures, and trust boundaries before sprint kickoff.
- **Automated Scanners:** Gate pull requests with automated SCA (Mend) & SAST checks.
- **Code Reviews:** Verify authorization checks and input sanitization as non-negotiable PR checkboxes.
- **Blameless Post-Mortems:** Treat security findings as learning opportunities to harden systems.

---

## 📰 Building a Security-First Mindset

Security is an ongoing engineering discipline, not a one-time lecture.

### Stay Informed & Sharp:
- **Internal Security Pulse:** Monthly security case studies and internal scan trends.
- **Top Industry Feeds & Reading:**
  - 🌐 [Krebs on Security](https://krebsonsecurity.com/) – Real-world breach breakdowns
  - 🌐 [Troy Hunt / Have I Been Pwned](https://www.troyhunt.com/) – Auth & data breach insights
  - 🌐 [CISA Known Exploited Vulnerabilities (KEV)](https://www.cisa.gov/known-exploited-vulnerabilities-catalog) – Catalog of exploited CVEs
  - 🌐 [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/) – Practical developer guides

---

<!-- _class: kahoot -->
## 🎮 INTERACTIVE QUIZ TIME! — Team Security Challenge

<div class="kahoot-card">
  <h2 style="color: #818cf8; border: none; font-size: 1.5em; margin-bottom: 0.6rem;">Self-Hosted Security Quiz (Kahoot Clone)</h2>
  <p style="color: #c7d2fe; font-size: 1.05em; margin-bottom: 1.4rem;">Company-safe, 100% private interactive quiz to test defensive coding skills!</p>
  
  <div style="background: rgba(129, 140, 248, 0.15); border: 2px dashed #818cf8; border-radius: 14px; padding: 1.2rem; max-width: 580px; margin: 0 auto;">
    <p style="font-size: 1.3em; font-weight: 800; color: #fff; margin-bottom: 0.6rem;">
      👉 <a href="http://localhost:5055" target="_blank" style="color: #38bdf8; text-decoration: underline;">Launch Security Quiz App (Port 5055)</a>
    </p>
    <p style="font-size: 1.0em; color: #cbd5e1; margin-bottom: 0.2rem;">Or via Master App: <a href="http://localhost:5050/quiz/" target="_blank" style="color: #a855f7;">http://localhost:5050/quiz/</a></p>
    <p style="font-size: 0.95em; color: #94a3b8; margin-top: 0.4rem;">Room PIN: <strong>849 201</strong> • Speed Scoring • Shape/Color Cards • Host Controls</p>
  </div>
  
  <p style="color: #94a3b8; font-size: 0.85em; margin-top: 1.2rem;">Topics: SSRF & IDOR (A01), Misconfiguration (A02), Supply Chain (A03), Cryptography (A04), Injection (A05), Insecure Design (A06), Auth (A07), Integrity (A08), SIEM Alerting (A09), Fail-Open Handling (A10).</p>
</div>

---

<!-- _class: lead -->
# 🚀 Hands-On: Master Security Application
### Interactive Vulnerability & Remediation Orchestration Suite (OWASP Top 10:2025)

---

## 🛡️ Master Application Live Demonstration

Experience all 10 OWASP Top 10 (2025) categories live with real-time exploit testing & defense verification:

- 🌐 **Open Master App:** [**http://localhost:5050**](http://localhost:5050)
- **Features in Dashboard:**
  - 🔴 **Vulnerable Demo:** Spawns live .NET service and executes attack in an internal `<iframe>`
  - 🟢 **Fixed Version:** Demonstrates remediated defense in the same iframe
  - 🎬 **Video Walkthroughs:** Automated Playwright recordings of attacks and defenses
- **Run Locally via Terminal:**
  ```bash
  cd MasterApp
  dotnet run --urls http://localhost:5050
  ```

---

## 📚 Appendix: OWASP Top 10 (2025) (Part 1: A01 - A05)

| OWASP (2025) | Key Risk Area | Primary Defensive Mitigation |
| :--- | :--- | :--- |
| **A01:2025 – Broken Access Control** | IDOR / Missing AuthZ / SSRF / Mass-Assignment | Resource ownership checks (`OwnerId == currentUserId`), strict IP egress filters, DTOs |
| **A02:2025 – Security Misconfiguration** | Rose to #2: Stack trace leaks, permissive CORS | RFC 7807 ProblemDetails, environment error boundaries, strict CORS whitelists, CSP |
| **A03:2025 – Software Supply Chain Failures** | Pipeline risks, typosquatting, unpinned packages | Automated SCA scanning (Mend), `<NuGetAudit>true</NuGetAudit>`, package pinning |
| **A04:2025 – Cryptographic Failures** | Moved to #4: Weak hashing (MD5), legacy DES/ECB | Salted PBKDF2 / Argon2id, Key Vault, authenticated AES-256-GCM |
| **A05:2025 – Injection** | Moved to #5: SQLi, OS Command Injection | Parameterized SQL queries (`@param`), safe native .NET APIs, input validation |

---

## 📚 Appendix: OWASP Top 10 (2025) (Part 2: A06 - A10)

| OWASP (2025) | Key Risk Area | Primary Defensive Mitigation |
| :--- | :--- | :--- |
| **A06:2025 – Insecure Design** | Moved to #6: Business logic flaws, coupon abuse | Threat modeling, atomic single-use constraints, server-authoritative pricing |
| **A07:2025 – Authentication Failures** | Credential stuffing, weak passwords, brute force | ASP.NET Core RateLimiting, account lockout policies, strong password rules |
| **A08:2025 – Software & Data Integrity** | Insecure deserialization, unverified webhooks | Type-safe JSON serialization, HMAC-SHA256 signature verification |
| **A09:2025 – Security Logging & Alerting** | Swallowed exceptions, missing SIEM alerts, leaked PII | Structured audit trails, sanitized telemetry, SIEM security alerts with correlation IDs |
| **A10:2025 – Mishandling of Exceptional Conditions** | Brand New: Failing open on error, unhandled states | Fail-Closed security defaults, safe exception boundaries, structured error responses |


---

<!-- _class: lead -->
# 🙋 Q & A / Discussion
### Thank You! Let's Build Secure Software Together.

Master App Dashboard: [http://localhost:5050](http://localhost:5050)
