# OWASP A09:2021 - Security Logging and Monitoring Failures (.NET Demo)

## Overview
Logging and monitoring failures allow attackers to maintain persistent access, tamper with systems, and extract data without detection.

## Vulnerabilities Demonstrated
1. **Cleartext Logging of Passwords and Tokens**: `logger.LogInformation` printing plaintext credentials and secrets.
2. **Missing Telemetry on Security Failures**: Failed login attempts return 401 without recording telemetry or alerting.
3. **Silent Exception Swallowing**: Swallowing security violation exceptions in financial transaction endpoints without audit logs.

## Review Skill
Run `security-code-review` to discover sensitive data leakage in logs and missing audit checkpoints.
