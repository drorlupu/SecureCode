# OWASP A04:2021 - Insecure Design (.NET Demo)

## Overview
Insecure Design represents architectural flaws where security requirements, threat modeling, and business logic constraints were omitted from the system design phase.

## Vulnerabilities Demonstrated
1. **Client-Controlled Pricing**: The checkout endpoint trusts the `ClientSpecifiedPrice` sent in the request JSON payload.
2. **Missing Input Domain Validation**: Negative order quantities allowed, resulting in negative total charges (refund theft).
3. **Missing Transactional Concurrency & Rate Limiting Controls**: Coupon redemption logic lacks state persistence and race condition protections.

## Review Skill
Run the `security-code-review` skill to evaluate business logic validation, state isolation, and rate-limiting patterns.
