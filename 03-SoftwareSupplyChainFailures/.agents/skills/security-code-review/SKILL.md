---
name: security-code-review
description: >-
  Performs an automated security code review on the 06-VulnerableAndOutdatedComponents project to identify
  OWASP A06:2021 (Vulnerable and Outdated Components) flaws, execute dependency vulnerability auditing,
  and recommend secure patched library versions.
---

# Security Code Review: OWASP A06:2021 - Vulnerable and Outdated Components

Use this skill to audit project dependencies (`.csproj`, `packages.lock.json`, `Directory.Packages.props`) against known vulnerability databases and NuGet security advisories.

## Review Steps & Detection Rules

### 1. Execute Automated Package Vulnerability Audit
- **Inspection Command**: Run `dotnet list package --vulnerable --include-transitive`.
- **Finding in `VulnerableAndOutdatedComponents.csproj`**:
  ```xml
  <PackageReference Include="Newtonsoft.Json" Version="12.0.1" />
  ```
- **Severity**: High (CVSS 7.5) - CWE-1104 (Use of Unmaintained Third Party Components) / CWE-1395.
- **Remediation**:
  1. Upgrade to the latest secure version of `Newtonsoft.Json` (13.0.3+) or migrate to built-in modern high-performance `System.Text.Json`:
     ```xml
     <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
     ```
  2. Enable `TreatWarningsAsErrors` and `NuGetAudit` in MSBuild / CI pipeline:
     ```xml
     <PropertyGroup>
       <NuGetAudit>true</NuGetAudit>
       <NuGetAuditMode>all</NuGetAuditMode>
       <NuGetAuditLevel>low</NuGetAuditLevel>
       <WarningsAsErrors>$(WarningsAsErrors);NU1901;NU1902;NU1903;NU1904</WarningsAsErrors>
     </PropertyGroup>
     ```
