# OWASP A04:2021 - Insecure Design (Remediated / Fixed Version)

## Applied Remediations
1. **Authoritative Server-Side Pricing**: Client only supplies `ProductId` and `Quantity`; unit price is retrieved directly from server catalog.
2. **Numeric Boundary Validation**: Constrains quantity to positive range (`1` to `100`), preventing negative price refund attacks.
3. **Atomic Single-Use State & Rate Limiting**: Uses thread-safe atomic collection to prevent coupon replay race conditions and applies ASP.NET Core RateLimiter middleware.
