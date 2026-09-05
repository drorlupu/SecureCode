# OWASP A06:2021 - Vulnerable and Outdated Components (Remediated / Fixed Version)

## Applied Remediations
1. **Patched Dependency**: Upgraded `Newtonsoft.Json` from `12.0.1` to patched release `13.0.3` (resolving GHSA-5crp-9r3c-p9vr).
2. **Automated Audit Properties**: Added `<NuGetAudit>true</NuGetAudit>` to MSBuild project file to fail or warn on vulnerable dependencies during restore and compilation.
