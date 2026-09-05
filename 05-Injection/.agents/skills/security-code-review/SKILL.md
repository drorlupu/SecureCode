---
name: security-code-review
description: >-
  Performs an automated security code review on the 03-Injection project to identify
  OWASP A03:2021 (Injection) vulnerabilities including SQL Injection and OS Command Injection.
---

# Security Code Review: OWASP A03:2021 - Injection

Use this skill to audit data flows and detect unsafe parameter concatenation into interpreters (SQL engines, OS shells, LDAP, XPath).

## Review Steps & Detection Rules

### 1. Audit SQL Query Construction
- **Inspection Rule**: Search for SQL queries built via string interpolation (`$""`), `string.Format`, or concatenation (`+`) passed to `cmd.CommandText`, `FromSqlRaw`, or `ExecuteSqlRaw`.
- **Finding in `Program.cs`**:
  ```csharp
  var sql = $"SELECT Id, Name, Price FROM Products WHERE Name LIKE '%{query}%'";
  cmd.CommandText = sql;
  ```
- **Severity**: Critical (CVSS 9.8) - CWE-89 (Improper Neutralization of Special Elements used in an SQL Command).
- **Remediation**: Use parameterized queries (`SqliteParameter` / `SqlParameter` / EF Core parameterized `FromSql`):
  ```csharp
  cmd.CommandText = "SELECT Id, Name, Price FROM Products WHERE Name LIKE @query";
  cmd.Parameters.AddWithValue("@query", $"%{query}%");
  ```

### 2. Audit Process / Shell Execution
- **Inspection Rule**: Search for `Process.Start`, `ProcessStartInfo` where `FileName` is a shell (`/bin/sh`, `/bin/bash`, `cmd.exe`, `powershell.exe`) or where arguments include unvalidated user input strings.
- **Finding in `Program.cs`**:
  ```csharp
  FileName = "/bin/sh",
  Arguments = $"-c \"ping -c 1 {host}\""
  ```
- **Severity**: Critical (CVSS 9.8) - CWE-78 (OS Command Injection).
- **Remediation**:
  1. Use built-in .NET network APIs like `System.Net.NetworkInformation.Ping` instead of spawning shell processes.
  2. If process execution is strictly necessary, avoid shell interpreters, validate arguments against strict regex/whitelists, and pass arguments via `ArgumentList.Add(...)` instead of concatenated strings.
