---
name: security-code-review
description: >-
  Performs an automated security code review on the 09-LoggingAndMonitoringFailures project to identify
  OWASP A09:2021 (Security Logging and Monitoring Failures) flaws including sensitive data logging,
  silent exception swallowing, and missing security audit trails.
---

# Security Code Review: OWASP A09:2021 - Security Logging & Monitoring Failures

Use this skill to audit logging statements, sensitive data sanitization, and security incident tracking.

## Review Steps & Detection Rules

### 1. Audit Logging of Sensitive Variables & PII
- **Inspection Rule**: Search for log statements printing variable names containing `Password`, `Pass`, `Secret`, `Token`, `Key`, `CreditCard`, `Cvv`, `Ssn`.
- **Finding in `Program.cs`**:
  ```csharp
  logger.LogInformation("User login attempt: Username={Username}, Password={Password}", payload.Username, payload.Password);
  ```
- **Severity**: High (CVSS 7.5) - CWE-532 (Insertion of Sensitive Information into Log File).
- **Remediation**: Never log plaintext credentials. Sanitize or mask sensitive fields, or log only metadata:
  ```csharp
  logger.LogInformation("Authentication attempt for user {Username}", payload.Username);
  ```

### 2. Audit Empty / Silent Catch Blocks on Security Failures
- **Inspection Rule**: Search for `catch` blocks that do not invoke logger error methods or rethrow.
- **Finding in `Program.cs`**:
  ```csharp
  catch (Exception)
  {
      return Results.BadRequest(new { Status = "Transfer failed." });
  }
  ```
- **Severity**: Medium (CVSS 6.5) - CWE-778 (Insufficient Logging) / CWE-391 (Unchecked Error Condition).
- **Remediation**: Log structured security warnings with correlation IDs and notify SIEM/telemetry systems:
  ```csharp
  catch (Exception ex)
  {
      logger.LogWarning(ex, "Security violation in transfer from account {FromAccount}", request.FromAccount);
      return Results.BadRequest(new { Status = "Transfer failed." });
  }
  ```
