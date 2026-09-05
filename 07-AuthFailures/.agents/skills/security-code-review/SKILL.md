---
name: security-code-review
description: >-
  Performs an automated security code review on the 07-AuthFailures project to identify
  OWASP A07:2021 (Identification and Authentication Failures) vulnerabilities including disabled JWT validation,
  insecure PRNG token generation, and missing account lockout/rate limiting.
---

# Security Code Review: OWASP A07:2021 - Identification & Authentication Failures

Use this skill to audit token validation parameters, session management, and authentication endpoints.

## Review Steps & Detection Rules

### 1. Audit JWT TokenValidationParameters
- **Inspection Rule**: Search for `TokenValidationParameters` with `ValidateIssuerSigningKey = false`, `ValidateLifetime = false`, `ValidateIssuer = false`, or `RequireSignedTokens = false`.
- **Finding in `Program.cs`**:
  ```csharp
  options.TokenValidationParameters = new TokenValidationParameters
  {
      ValidateIssuer = false,
      ValidateAudience = false,
      ValidateLifetime = false,
      ValidateIssuerSigningKey = false
  };
  ```
- **Severity**: Critical (CVSS 9.8) - CWE-287 (Improper Authentication) / CWE-347 (Improper Verification of Cryptographic Signature).
- **Remediation**:
  ```csharp
  options.TokenValidationParameters = new TokenValidationParameters
  {
      ValidateIssuer = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"],
      ValidateAudience = true,
      ValidAudience = builder.Configuration["Jwt:Audience"],
      ValidateLifetime = true,
      ClockSkew = TimeSpan.FromMinutes(1),
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
  };
  ```

### 2. Audit Random / Session Generation
- **Inspection Rule**: Search for `System.Random` used in security-sensitive contexts (tokens, reset codes, MFA nonces).
- **Finding in `Program.cs`**:
  ```csharp
  var random = new Random();
  var sessionToken = $"SESSION-{username}-{random.Next(100000, 999999)}";
  ```
- **Severity**: High (CVSS 7.5) - CWE-330 (Use of Insufficiently Random Values).
- **Remediation**: Use `RandomNumberGenerator`:
  ```csharp
  var tokenBytes = RandomNumberGenerator.GetBytes(32);
  var secureToken = Convert.ToBase64String(tokenBytes);
  ```
