---
name: security-code-review
description: >-
  Identify security vulnerabilities and suggest secure coding practices. Use this skill when
  reviewing code for security vulnerabilities, performing security audits, analyzing
  authentication/authorization code, checking for OWASP Top 10 vulnerabilities, or analyzing
  cryptographic implementations.
---

# Security Code Review Guidelines (Smithery: kousen/security-code-review)

When reviewing code for security issues, systematically check for common vulnerabilities and suggest secure alternatives using the OWASP Top 10 focus areas, checklist, and standardized reporting format.

## OWASP Top 10 Focus Areas & Detection Rules

### 1. Injection Attacks (SQL, NoSQL, Command, LDAP)
**Look for**:
- String concatenation or interpolation in SQL queries (`$""`, `+`, `string.Format`, `cmd.CommandText`, `FromSqlRaw`)
- Unsanitized user input in database queries
- Direct execution of user input in system shells (`Process.Start`, `ProcessStartInfo`, `Runtime.getRuntime().exec`)

### 2. Broken Authentication & Session Management
**Look for**:
- Passwords stored in plain text or with weak hashes (MD5, SHA1)
- Weak password requirements or lack of lockout / rate-limiting
- Predictable session IDs or security tokens (`System.Random`, `Math.random()`)
- Disabled token signature validation (`ValidateIssuerSigningKey = false`, `ValidateLifetime = false`)

### 3. Sensitive Data Exposure & Cryptographic Failures
**Look for**:
- Logging sensitive information (passwords, tokens, credit card numbers, SSNs)
- Storing secrets/passwords hardcoded in source code or `appsettings.json`
- Use of broken or weak encryption algorithms/modes (DES, RC4, AES-ECB)
- Detailed error messages leaking system internals in production (`app.UseDeveloperExceptionPage()`)

### 4. Broken Access Control & IDOR
**Look for**:
- Missing authorization checks on endpoints (`[Authorize]`, `.RequireAuthorization()`)
- Insecure direct object references (IDOR/BOLA): retrieving user resources using IDs from URL routes without validating caller identity/tenant ownership
- Mass-assignment / over-posting: binding request bodies directly to privileged domain entities (`Role`, `IsAdmin`)
- Path traversal vulnerabilities in file handling

### 5. Insecure Design & Business Logic Flaws
**Look for**:
- Blindly trusting client-controlled business parameters (e.g., client-specified unit prices, discount calculations)
- Lack of numeric bounds checking allowing negative quantities (negative pricing exploits)
- Missing transaction isolation and rate limiting on sensitive business workflows

### 6. Security Misconfiguration
**Look for**:
- Debug mode or developer exception pages enabled in non-development environments
- Overly permissive CORS policies (`SetIsOriginAllowed(_ => true)`, wildcard origins with credentials)
- Missing security headers (`Content-Security-Policy`, `X-Content-Type-Options: nosniff`, `X-Frame-Options`, `Strict-Transport-Security`)
- Default administrative credentials in configuration files

### 7. Software and Data Integrity Failures & Insecure Deserialization
**Look for**:
- Deserializing untrusted data with polymorphic type handling enabled (`TypeNameHandling.All`, `TypeNameHandling.Auto`, `BinaryFormatter`, `ObjectInputStream`)
- Executing or fetching remote scripts/plugins without cryptographic signature or hash verification

### 8. Vulnerable and Outdated Components
**Look for**:
- Outdated dependencies with known CVEs (`dotnet list package --vulnerable`, `npm audit`, `pip-audit`)
- Missing automated package vulnerability audit enforcement in build configurations

### 9. Insufficient Logging and Monitoring Failures
**Look for**:
- Missing audit logs for security-critical events (failed logins, privilege escalations, financial transactions)
- Silent exception swallowing (`catch (Exception) { /* empty */ }`) preventing incident detection
- Plaintext secrets or PII included in logging statements

### 10. Server-Side Request Forgery (SSRF)
**Look for**:
- Outbound HTTP requests (`HttpClient.GetAsync`, `WebRequest`, `fetch`) where destination URL is supplied or influenced by untrusted users
- Missing validation against internal IP addresses (loopback `127.0.0.1`, RFC 1918 private ranges, cloud metadata `169.254.169.254`)

---

## Security Review Checklist

When reviewing code, verify:
- [ ] Input validation on all user inputs
- [ ] Output encoding for all user-controlled data
- [ ] Parameterized queries for all database access
- [ ] Authentication on all protected resources
- [ ] Authorization checks before accessing resources
- [ ] Secure password storage (Argon2, PBKDF2, bcrypt)
- [ ] HTTPS and secure transport
- [ ] Security response headers configured
- [ ] Error handling that does not leak internal stack traces or connection strings
- [ ] Structured logging of security-relevant events without logging secrets
- [ ] Rate limiting on public APIs and authentication routes
- [ ] Updated dependencies without known CVEs
- [ ] Secrets stored in environment variables, Key Vault, or secrets manager
- [ ] Type-safe, constrained deserialization without arbitrary type resolution

---

## Standardized Reporting Format

Document every finding using the official template:

```markdown
### [SEVERITY] <Issue Title>

**Location**: <FilePath>:<LineNumber>

**Issue**: <Concise explanation of the flaw>

**Vulnerable Code**:
```<language>
// Vulnerable snippet
```

**Impact**: <Attack vector and consequences if exploited>

**Recommendation**: <Defensive guidance and best practice>

**Fixed Code**:
```<language>
// Remediated code
```

**Severity**: Critical | High | Medium | Low
**CVSS Score**: <Score> (<Rating>)
**Remediation Priority**: Immediate | High | Medium | Low
```
