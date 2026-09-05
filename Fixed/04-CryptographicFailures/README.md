# OWASP A02:2021 - Cryptographic Failures (Remediated / Fixed Version)

## Applied Remediations
1. **Password Hashing Resolved**: Upgraded from MD5 to `Microsoft.AspNetCore.Identity.PasswordHasher<T>` implementing salted PBKDF2-HMAC-SHA512 with 100,000 iterations.
2. **Cipher Mode & Hardcoded Key Resolved**: Replaced static key and ECB mode with authenticated AES-256-GCM (`AesGcm`) utilizing cryptographically random 12-byte nonces and 16-byte authentication tags.
