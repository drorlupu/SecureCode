# OWASP A08:2021 - Software and Data Integrity Failures (.NET Demo)

## Overview
Software and data integrity failures relate to code and infrastructure that do not protect against integrity violations, such as untrusted deserialization or unverified plugins.

## Vulnerabilities Demonstrated
1. **Insecure Deserialization via `TypeNameHandling.All`**: Allows arbitrary object type instantiation leading to Remote Code Execution.
2. **Unsigned Remote Module Loading**: Fetching external scripts/plugins without cryptographic signature or checksum verification.

## Review Skill
Run `security-code-review` to detect dangerous deserialization options and unverified data sinks.
