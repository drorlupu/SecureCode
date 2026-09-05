using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// REMEDIATION: Server-Side Request Forgery (SSRF) FIXED
// Enforces scheme validation (HTTP/HTTPS), resolves host DNS, and explicitly blocks
// loopback (127.0.0.1, ::1), AWS/GCP/Azure link-local metadata (169.254.169.254),
// and private RFC 1918 internal network addresses.
// =========================================================================================
app.MapPost("/api/avatar/fetch-remote", async (IHttpClientFactory clientFactory, AvatarFetchRequest request) =>
{
    // Step 1: Scheme and format validation
    if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest(new { Message = "Invalid URL scheme. Only HTTP and HTTPS are permitted." });
    }

    // Step 2: Prevent port scanning against internal non-standard services
    if (uri.Port != 80 && uri.Port != 443)
    {
        return Results.BadRequest(new { Message = "Only standard ports 80 and 443 are permitted." });
    }

    try
    {
        // Step 3: Resolve DNS and validate IP address
        var hostAddresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost);
        if (hostAddresses.Length == 0)
        {
            return Results.BadRequest(new { Message = "Could not resolve hostname." });
        }

        foreach (var ip in hostAddresses)
        {
            if (IsRestrictedInternalIp(ip))
            {
                return Results.Problem(
                    detail: "Access to private, loopback, or cloud metadata IP ranges is prohibited.",
                    statusCode: 403,
                    title: "SSRF Protection Triggered");
            }
        }

        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        // Fetch securely
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            return Results.BadRequest(new { Message = "Remote server returned an error." });
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        // Verify image media type
        if (contentType == null || !contentType.StartsWith("image/"))
        {
            return Results.BadRequest(new { Message = "Content-Type must be an image." });
        }

        var contentBytes = await response.Content.ReadAsByteArrayAsync();
        return Results.Ok(new
        {
            Message = "Remote avatar validated and fetched securely.",
            ContentType = contentType,
            Size = contentBytes.Length
        });
    }
    catch (SocketException)
    {
        return Results.BadRequest(new { Message = "Unable to connect to specified host." });
    }
});

app.Run();

static bool IsRestrictedInternalIp(IPAddress ip)
{
    // Check loopback (127.0.0.1, ::1)
    if (IPAddress.IsLoopback(ip)) return true;

    // Convert IPv4-mapped IPv6 to IPv4
    if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();

    byte[] bytes = ip.GetAddressBytes();

    // Check IPv4 private and link-local ranges
    if (ip.AddressFamily == AddressFamily.InterNetwork)
    {
        // 10.0.0.0/8 (RFC 1918)
        if (bytes[0] == 10) return true;

        // 172.16.0.0/12 (RFC 1918)
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

        // 192.168.0.0/16 (RFC 1918)
        if (bytes[0] == 192 && bytes[1] == 168) return true;

        // 169.254.0.0/16 (Link-local & AWS/Azure/GCP metadata)
        if (bytes[0] == 169 && bytes[1] == 254) return true;

        // 0.0.0.0/8
        if (bytes[0] == 0) return true;
    }
    else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
    {
        // IPv6 link-local
        if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) return true;
    }

    return false;
}

public record AvatarFetchRequest(string ImageUrl);
