---
name: security-code-review
description: >-
  Performs an automated security code review on the 05-SecurityMisconfiguration project to identify
  OWASP A05:2021 (Security Misconfiguration) flaws including insecure CORS, unconditional developer exception pages,
  missing security headers, and default credentials in config.
---

# Security Code Review: OWASP A05:2021 - Security Misconfiguration

Use this skill to audit middleware setup, CORS configurations, error-handling pipelines, and configuration files for misconfigurations.

## Review Steps & Detection Rules

### 1. Audit Developer Exception Page & Environment Guards
- **Inspection Rule**: Look for `app.UseDeveloperExceptionPage()` called without guarding with `if (app.Environment.IsDevelopment())`.
- **Finding in `Program.cs`**:
  ```csharp
  app.UseDeveloperExceptionPage(); // Always runs, even in Staging/Production
  ```
- **Severity**: High (CVSS 7.5) - CWE-209 (Generation of Error Message Containing Sensitive Information).
- **Remediation**:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseDeveloperExceptionPage();
  }
  else
  {
      app.UseExceptionHandler("/error");
      app.UseHsts();
  }
  ```

### 2. Audit CORS Policies
- **Inspection Rule**: Look for `SetIsOriginAllowed(_ => true)` or `.AllowAnyOrigin()` paired with `.AllowCredentials()`.
- **Finding in `Program.cs`**:
  ```csharp
  options.AddDefaultPolicy(policy => policy.SetIsOriginAllowed(_ => true).AllowCredentials());
  ```
- **Severity**: Medium (CVSS 6.5) - CWE-942 (Permissive Cross-origin Resource Sharing Policy).
- **Remediation**: Explicitly specify trusted origins (e.g. `WithOrigins("https://app.mydomain.com")`).

### 3. Audit Security Headers & Configuration Secrets
- **Inspection Rule**:
  - Verify presence of `app.UseHsts()`, Content Security Policy (CSP), `X-Content-Type-Options`, `X-Frame-Options`.
  - Check `appsettings.json` for default admin credentials and unencrypted secrets.
- **Remediation**: Add security middleware headers and migrate secrets to Azure Key Vault / Environment Variables.
