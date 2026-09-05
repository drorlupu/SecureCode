# OWASP A10:2021 - Server-Side Request Forgery (SSRF) (.NET Demo)

## Overview
SSRF flaws occur whenever a web application fetches a remote resource without validating the user-supplied URL, allowing an attacker to coerce the application to send crafted requests to unintended internal destinations or cloud metadata services.

## Vulnerabilities Demonstrated
1. **Unrestricted Remote URL Fetching**: `POST /api/avatar/fetch-remote` calls `HttpClient.GetAsync` on user input without URL or IP validation.
2. **Access to Internal Subnet Services & Cloud Metadata**: Endpoints can be exploited to reach `http://169.254.169.254` or internal admin endpoints (`/internal/admin/cloud-credentials`).

## Review Skill
Execute `security-code-review` to see how static analysis flags unvalidated `HttpClient` calls and enforces DNS resolution validation.
