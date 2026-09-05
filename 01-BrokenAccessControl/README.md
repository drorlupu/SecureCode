# OWASP A01:2021 - Broken Access Control (.NET Demo)

## Overview
Access control enforces policy such that users cannot act outside of their intended permissions. Failures typically lead to unauthorized information disclosure, modification, or destruction of all data or performing a business function outside the user's limits.

---

## Vulnerabilities Demonstrated
1. **Insecure Direct Object Reference (IDOR / BOLA)**: `GET /api/documents/{id}` allows any caller to view other users' confidential documents by simply supplying their GUID.
2. **Missing Function-Level Access Control**: `POST /api/admin/system-reset` lacks authorization requirements.
3. **Mass Assignment / Privilege Escalation**: `PUT /api/users/profile` allows users to elevate their `Role` to `Administrator`.

---

## Running the Security Code Review Skill
Execute the `security-code-review` skill to analyze this project:
- Antigravity will parse the endpoints in `Program.cs`, identify the missing authorization checks and mass-assignment flaws, and generate safe remediations.

---

## Automated Playwright Video Recording & UI Demonstration

An interactive UI and an automated Playwright recording script are provided to demonstrate and record the vulnerability in action.

### Key Files
- **Interactive UI**: [`wwwroot/index.html`](./wwwroot/index.html)
- **Playwright Recording Script**: [`record_demo.js`](./record_demo.js)
- **Output Video Directory**: [`recordings/owasp-a01-broken-access-control.webm`](./recordings/owasp-a01-broken-access-control.webm)

### Prerequisites
- .NET 10 SDK
- Node.js (v18+)
- Playwright (`npm install playwright` and `npx playwright install chromium`)

### How to Run and Record the Demonstration

#### 1. Start the Vulnerable Web App
Run the application on port `5005`:
```bash
dotnet run --project BrokenAccessControl.csproj --urls http://localhost:5005
```

#### 2. Execute the Playwright Movie Recording
In another terminal window:
```bash
cd 01-BrokenAccessControl
node record_demo.js
```

The script will:
1. Launch a Chromium browser session with video recording enabled (`1280x720`).
2. Navigate to `http://localhost:5005`.
3. **Step 1**: Demonstrate authorized baseline access (Alice accessing her own tax return).
4. **Step 2**: Demonstrate IDOR (Alice requesting Bob's document GUID, exposing confidential medical records).
5. **Step 3**: Demonstrate Missing Function-Level Access Control (Standard user triggering administrative system reset).
6. **Step 4**: Demonstrate Role Mass-Assignment (Alice elevating her account role to Administrator via profile update).
7. Finalize and save the video to `recordings/owasp-a01-broken-access-control.webm`.

---

## Remediated Version
For the secure implementation resolving all three vulnerabilities, see:
[`../Fixed/01-BrokenAccessControl/`](../Fixed/01-BrokenAccessControl/)
