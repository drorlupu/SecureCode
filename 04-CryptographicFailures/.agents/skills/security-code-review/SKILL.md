---
name: security-code-review
description: >-
  Performs an automated security code review on the 02-CryptographicFailures project to identify
  OWASP A02:2021 (Cryptographic Failures) flaws including MD5 password hashing, hardcoded encryption keys,
  and ECB mode block ciphers.
---

# Security Code Review: OWASP A02:2021 - Cryptographic Failures

Use this skill to perform a static code review of cryptographic operations in `02-CryptographicFailures`.

## Review Steps & Detection Rules

### 1. Audit Password Hashing Algorithms
- **Inspection Rule**: Search for usages of `MD5.Create()`, `SHA1.Create()`, `SHA256.Create()`, or `HMAC` directly for password storage without adaptive key derivation (work factor + salt).
- **Finding in `Program.cs`**:
  ```csharp
  using var md5 = MD5.Create();
  var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
  ```
- **Severity**: High (CVSS 7.5) - CWE-328 (Use of Weak Hash) / CWE-916 (Use of Password Hash With Insufficient Computational Effort).
- **Remediation**: Use `Microsoft.AspNetCore.Identity.PasswordHasher<T>` (PBKDF2 with HMAC-SHA512) or Argon2id / BCrypt:
  ```csharp
  var hasher = new PasswordHasher<string>();
  string secureHash = hasher.HashPassword(request.Username, request.Password);
  PasswordVerificationResult result = hasher.VerifyHashedPassword(request.Username, storedHash, request.Password);
  ```

### 2. Audit Symmetric Encryption Mode & Key Management
- **Inspection Rule**: Search for `CipherMode.ECB`, hardcoded byte keys in source code, or DES/3DES ciphers.
- **Finding in `Program.cs`**:
  ```csharp
  var HardcodedKey = Encoding.UTF8.GetBytes("Hardcoded1234567");
  aes.Mode = CipherMode.ECB;
  ```
- **Severity**: High (CVSS 7.4) - CWE-327 (Broken Crypto Algorithm) / CWE-321 (Hard-coded Cryptographic Key).
- **Remediation**: Use authenticated encryption (AES-256-GCM / `AesGcm`) or ASP.NET Core Data Protection API (`IDataProtector`), loading keys securely from Azure Key Vault, AWS Secrets Manager, or user-secrets in development.
