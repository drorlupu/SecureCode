---
name: security-code-review
description: >-
  Performs an automated security code review on the 08-SoftwareAndDataIntegrityFailures project to identify
  OWASP A08:2021 (Software and Data Integrity Failures) vulnerabilities including Insecure Deserialization
  via TypeNameHandling.All and unverified remote resource loading.
---

# Security Code Review: OWASP A08:2021 - Software and Data Integrity Failures

Use this skill to audit serialization mechanisms, object binders, and external package/resource verification pipelines.

## Review Steps & Detection Rules

### 1. Audit Deserialization Settings for Type Handling
- **Inspection Rule**: Search for `TypeNameHandling.All`, `TypeNameHandling.Auto`, `TypeNameHandling.Objects`, `BinaryFormatter.Deserialize`, or `NetDataContractSerializer`.
- **Finding in `Program.cs`**:
  ```csharp
  var settings = new JsonSerializerSettings
  {
      TypeNameHandling = TypeNameHandling.All
  };
  var state = JsonConvert.DeserializeObject(serializedState, settings);
  ```
- **Severity**: Critical (CVSS 9.8) - CWE-502 (Deserialization of Untrusted Data).
- **Remediation**:
  1. Set `TypeNameHandling = TypeNameHandling.None` (default).
  2. Use strongly-typed DTO deserialization using `System.Text.Json.JsonSerializer.Deserialize<T>()`.
  3. If polymorphic deserialization is required, use safe discriminated type discriminators with strict whitelisting (`JsonDerivedTypeAttribute`).

### 2. Audit Code/Asset Integrity Verification
- **Inspection Rule**: Check remote file/plugin loaders for cryptographic hash (SHA-256) or digital signature verification (Authenticode / GPG).
- **Remediation**: Verify cryptographic signatures and hashes before processing or executing remote payloads.
