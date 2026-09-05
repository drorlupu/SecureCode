---
name: security-code-review
description: >-
  Performs an automated security code review on the 10-ServerSideRequestForgery project to identify
  OWASP A10:2021 (Server-Side Request Forgery) flaws, detect unvalidated outbound HTTP requests,
  and implement IP whitelist / loopback protection defenses.
---

# Security Code Review: OWASP A10:2021 - Server-Side Request Forgery (SSRF)

Use this skill to audit outbound HTTP calls (`HttpClient`, `WebRequest`, `WebClient`) for missing URL and IP address validation.

## Review Steps & Detection Rules

### 1. Audit Outbound HTTP Client Invocations
- **Inspection Rule**: Search for `HttpClient.GetAsync`, `PostAsync`, `SendAsync`, or `GetStringAsync` called with dynamic URLs derived from request inputs without host/IP validation.
- **Finding in `Program.cs`**:
  ```csharp
  var response = await client.GetAsync(request.ImageUrl);
  ```
- **Severity**: Critical (CVSS 8.6) - CWE-918 (Server-Side Request Forgery).
- **Remediation**:
  1. Validate scheme (`http`/`https` only) and enforce an allowed domain whitelist when possible.
  2. Resolve DNS and verify that resolved IP addresses are not loopback (`127.0.0.0/8`, `::1`), private RFC 1918 (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`), link-local/cloud metadata (`169.254.169.254`), or multicast.
  3. Example safe verification helper:
     ```csharp
     if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uri) || 
         (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
     {
         return Results.BadRequest("Invalid URL scheme");
     }

     var hostEntry = await Dns.GetHostEntryAsync(uri.DnsSafeHost);
     foreach (var ip in hostEntry.AddressList)
     {
         if (IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal || ip.ToString().StartsWith("169.254.") || ip.ToString().StartsWith("10."))
         {
             return Results.BadRequest("Access to internal/private IP ranges is prohibited");
         }
     }
     ```
