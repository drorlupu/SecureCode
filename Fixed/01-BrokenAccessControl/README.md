# OWASP A01:2021 - Broken Access Control (Remediated / Fixed Version)

## Overview
This is the secure, hardened implementation of `01-BrokenAccessControl`. It demonstrates how the three access control flaws are neutralized in ASP.NET Core using defense-in-depth principles.

---

## Remediations Applied & Verified

### 1. Insecure Direct Object Reference (IDOR / BOLA) Resolved
- **Defense Mechanism**: The endpoint `/api/documents/{id}` extracts the authenticated caller's identifier from `ClaimTypes.NameIdentifier` and enforces resource ownership:
  ```csharp
  var doc = documents.FirstOrDefault(d => d.Id == id && d.OwnerId == currentUserId);
  if (doc == null) return Results.NotFound(new { Message = "Document not found or access denied." });
  ```
- **Observed Behavior**: When Alice attempts to request Bob's document GUID, the server returns `404 Not Found / Access Denied`. Bob's records remain confidential.

### 2. Function-Level Access Control Resolved
- **Defense Mechanism**: The administrative reset endpoint `/api/admin/system-reset` is protected with `.RequireAuthorization("AdminOnly")` requiring the `Admin` role claim:
  ```csharp
  builder.Services.AddAuthorization(options => {
      options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
  });
  ```
- **Observed Behavior**: Standard user sessions attempting to trigger the reset receive `HTTP 403 Forbidden`.

### 3. Role Mass-Assignment Resolved
- **Defense Mechanism**: The profile update endpoint binds to a constrained Data Transfer Object [`UpdateProfileDto`](./Program.cs) exposing only editable fields (`FullName`, `Email`):
  ```csharp
  public record UpdateProfileDto(string FullName, string Email);
  ```
- **Observed Behavior**: Injected `"Role": "Administrator"` JSON properties are discarded during deserialization; the user's role remains unchanged.

---

## Interactive UI & Playwright Video Recording

Just like the vulnerable version, a dedicated defensive web UI and an automated Playwright recording script are provided.

### Key Files
- **Remediated Web UI**: [`wwwroot/index.html`](./wwwroot/index.html)
- **Playwright Recording Script**: [`record_demo.js`](./record_demo.js)
- **Recorded Defense Video**: [`recordings/owasp-a01-broken-access-control-fixed.webm`](./recordings/owasp-a01-broken-access-control-fixed.webm)

### Running the Remediated UI and Recording

#### 1. Start the Remediated Server
```bash
dotnet run --project BrokenAccessControl.Fixed.csproj --urls http://localhost:5006
```

#### 2. Open in Browser or Run Playwright
- **Browser**: Navigate to `http://localhost:5006/` to interact with the defensive UI and user switcher.
- **Automated Recording**:
  ```bash
  cd Fixed/01-BrokenAccessControl
  node record_demo.js
  ```
