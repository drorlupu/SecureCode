# OWASP A05:2021 - Security Misconfiguration (.NET Demo)

## Overview
Security misconfiguration is the most commonly seen issue. It happens when security settings are defined, implemented, and maintained with default or insecure values, verbose error messaging is enabled, or unnecessary features and permissions are enabled.

## Vulnerabilities Demonstrated
1. **Unconditional Developer Exception Page**: `app.UseDeveloperExceptionPage()` enabled outside development, leaking sensitive database error messages and stack traces.
2. **Overly Permissive CORS**: `SetIsOriginAllowed(_ => true)` with credentials.
3. **Default Admin Credentials in `appsettings.json`**: Plaintext credentials stored in version control config files.
4. **Missing Security Response Headers**: Missing HSTS, CSP, and Anti-Clickjacking headers.

## Review Skill
Execute the `security-code-review` skill to scan pipeline middleware and configuration files.
