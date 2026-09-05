# OWASP A07:2021 - Identification and Authentication Failures (.NET Demo)

## Overview
Confirmation of the user's identity, authentication, and session management is critical to protect against authentication-related attacks.

## Vulnerabilities Demonstrated
1. **Disabled JWT Validation**: `ValidateIssuerSigningKey = false` allowing attackers to forge arbitrary tokens.
2. **Predictable Session Generation**: `System.Random` used for authentication tokens.
3. **No Rate Limiting or Brute Force Protection**: Password login endpoint has no backoff or lockout mechanism.

## Review Skill
Run `security-code-review` to inspect token validation settings and randomness generators.
