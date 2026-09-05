# OWASP A06:2021 - Vulnerable and Outdated Components (.NET Demo)

## Overview
Vulnerable and Outdated Components occurs when third-party libraries, frameworks, or dependencies are not kept up-to-date, or when known CVEs exist in used packages.

## Vulnerabilities Demonstrated
1. **Outdated Third-Party Dependency**: Project references `Newtonsoft.Json 12.0.1` instead of secure patched versions.
2. **Missing Automated NuGet Audit Controls**: Lack of build-time `<NuGetAudit>` configuration.

## Review Skill
Execute `security-code-review` to discover package vulnerabilities and learn how to automate security updates using `dotnet list package --vulnerable`.
