# OWASP A03:2021 - Injection (.NET Demo)

## Overview
Injection flaws, such as SQL, NoSQL, OS Command, and LDAP injection, occur when untrusted data is sent to an interpreter as part of a command or query. Hostile data tricks the interpreter into executing unintended commands or accessing data without proper authorization.

## Vulnerabilities Demonstrated
1. **SQL Injection (SQLi)**: `GET /api/products/search?query=...` interpolates raw user input into an SQLite query string.
2. **OS Command Injection**: `GET /api/tools/ping?host=...` directly concats user input into `/bin/sh -c "ping -c 1 {host}"`.

## Review Skill
Execute the `security-code-review` skill to see how static analysis identifies string concatenation in SQL queries and unsafe `Process.Start` argument construction.
