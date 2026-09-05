# OWASP A02:2021 - Cryptographic Failures (.NET Demo)

## Overview
Formerly known as *Sensitive Data Exposure*, Cryptographic Failures refer to flaws related to cryptography (or lack thereof), which often lead to exposure of sensitive data like credentials, payment cards, or health records.

## Vulnerabilities Demonstrated
1. **Weak / Insecure Password Hashing**: Fast unsalted MD5 hash used for user passwords.
2. **Hardcoded Encryption Key**: Symmetric key hardcoded directly into application source code.
3. **Insecure Cipher Mode (AES-ECB)**: AES configured with Electronic Codebook (ECB) mode, leaking plaintext data patterns.

## Review Skill
Run the `security-code-review` skill to scan the codebase for deprecated crypto algorithms, hardcoded keys, and insecure cipher modes.
