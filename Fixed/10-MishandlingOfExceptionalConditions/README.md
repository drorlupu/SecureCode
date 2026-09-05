# OWASP A10:2021 - Server-Side Request Forgery (Remediated / Fixed Version)

## Applied Remediations
1. **Protocol & Port Enforcement**: Restricts inbound URLs to standard HTTP (`80`) and HTTPS (`443`) schemes.
2. **DNS Resolution IP Filtering**: Resolves destination hostnames and blocks connections to loopback (`127.0.0.1`), private RFC 1918 subnets (`10.x`, `172.16-31.x`, `192.168.x`), and cloud metadata IP ranges (`169.254.169.254`).
3. **Response Validation**: Enforces valid `image/*` media types and sets strict timeouts.
