---
name: security-code-review
description: >-
  Performs an automated security code review on the 04-InsecureDesign project to identify
  OWASP A04:2021 (Insecure Design) flaws including client-controlled business parameters,
  negative quantity pricing manipulation, and missing business logic transaction controls.
---

# Security Code Review: OWASP A04:2021 - Insecure Design

Use this skill to audit application workflows, business logic assumptions, and architectural design flaws.

## Review Steps & Detection Rules

### 1. Audit Trust in Client-Supplied Financial / Business Parameters
- **Inspection Rule**: Identify endpoints where business values (e.g. price, discounts, access limits) are accepted directly from client DTOs instead of calculating them from authoritative server-side models.
- **Finding in `Program.cs`**:
  ```csharp
  decimal itemPrice = request.ClientSpecifiedPrice;
  decimal total = itemPrice * quantity;
  ```
- **Severity**: High (CVSS 8.3) - CWE-840 (Business Logic Errors) / CWE-20 (Improper Input Validation).
- **Remediation**:
  1. Never trust pricing or permissions sent by the client. Fetch price from the internal database catalog:
     ```csharp
     if (!productCatalog.TryGetValue(request.ProductId, out var product)) return Results.BadRequest("Invalid product");
     if (request.Quantity <= 0 || request.Quantity > 100) return Results.BadRequest("Invalid quantity range");
     decimal total = product.AuthoritativePrice * request.Quantity;
     ```

### 2. Audit Race Conditions and Anti-Abuse Controls in Promotion Workflows
- **Inspection Rule**: Check promotion/coupon code redemption endpoints for idempotent transaction boundaries, user claiming history, and rate limiting.
- **Finding in `Program.cs`**:
  ```csharp
  if (request.CouponCode == "SAVE50") { ... }
  ```
- **Severity**: Medium (CVSS 6.5) - CWE-799 (Improper Control of Generation of Code or Resource Consumption).
- **Remediation**: Implement ASP.NET Core RateLimiter middleware, record user-to-coupon claims in transactional database tables, and apply distributed locks/atomic transactions to prevent race condition double-spending.
