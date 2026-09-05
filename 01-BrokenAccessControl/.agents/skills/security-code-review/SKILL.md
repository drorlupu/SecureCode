---
name: security-code-review
description: >-
  Performs an automated security code review on the 01-BrokenAccessControl project to identify
  OWASP A01:2021 (Broken Access Control) vulnerabilities including IDOR/BOLA, Missing Function Level
  Access Control, and Mass Assignment.
---

# Security Code Review: OWASP A01:2021 - Broken Access Control

Use this skill to perform a systematic static code review of the `01-BrokenAccessControl` project, diagnose authorization weaknesses, demonstrate attack scenarios, and output the remediated secure code.

## Review Steps & Detection Rules

### 1. Audit Endpoint Authorization & IDOR (`/api/documents/{id}`)
- **Inspection Rule**: Look for route parameters identifying sensitive entities (e.g. `{id:guid}`) where data is fetched directly without checking `user.FindFirstValue(ClaimTypes.NameIdentifier) == doc.OwnerId` or invoking an `IAuthorizationService`.
- **Finding in `Program.cs`**:
  ```csharp
  app.MapGet("/api/documents/{id:guid}", (Guid id, ClaimsPrincipal user) =>
  {
      var doc = documents.FirstOrDefault(d => d.Id == id);
      return Results.Ok(doc); // IDOR: Missing check that doc.OwnerId matches caller
  });
  ```
- **Severity**: Critical (CVSS 8.6)
- **Remediation**:
  ```csharp
  app.MapGet("/api/documents/{id:guid}", (Guid id, ClaimsPrincipal user) =>
  {
      var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(currentUserId)) return Results.Unauthorized();

      var doc = documents.FirstOrDefault(d => d.Id == id && d.OwnerId == currentUserId);
      if (doc == null) return Results.NotFound();

      return Results.Ok(doc);
  }).RequireAuthorization();
  ```

### 2. Audit Administrative Function-Level Access Control (`/api/admin/system-reset`)
- **Inspection Rule**: Look for sensitive administrative routes that lack `.RequireAuthorization("AdminPolicy")` or `[Authorize(Roles = "Admin")]`.
- **Finding in `Program.cs`**:
  ```csharp
  app.MapPost("/api/admin/system-reset", () => { ... }); // Missing RequireAuthorization
  ```
- **Severity**: High (CVSS 7.5)
- **Remediation**:
  ```csharp
  app.MapPost("/api/admin/system-reset", () =>
  {
      return Results.Ok(new { Status = "System reset completed." });
  }).RequireAuthorization(policy => policy.RequireRole("Admin"));
  ```

### 3. Audit Mass-Assignment / Over-Posting (`/api/users/profile`)
- **Inspection Rule**: Check if request payloads bind directly to sensitive domain entities containing privilege fields like `Role`, `IsAdmin`, or `Permissions`.
- **Finding in `Program.cs`**:
  ```csharp
  existing.Role = updatedProfile.Role; // Attacker can supply {"role": "Admin"}
  ```
- **Severity**: High (CVSS 8.1)
- **Remediation**: Use dedicated Input DTOs (Data Transfer Objects) that only expose allowed editable fields (`FullName`, `Email`) and never the `Role`.
