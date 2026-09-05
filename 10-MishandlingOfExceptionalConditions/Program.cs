using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient factory
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// VULNERABILITY #1: Unvalidated Server-Side Request Forgery (SSRF) (OWASP A10:2021)
// The API accepts a user-controlled URL to fetch remote avatars/webhooks.
// Because it does not validate the host/IP against private or internal addresses,
// an attacker can probe cloud metadata (http://169.254.169.254) or internal services (http://localhost:5000).
// =========================================================================================
app.MapPost("/api/avatar/fetch-remote", async (IHttpClientFactory clientFactory, AvatarFetchRequest request) =>
{
    var client = clientFactory.CreateClient();

    // [CRITICAL VULNERABILITY]: Fetching arbitrary user-supplied URL directly!
    var response = await client.GetAsync(request.ImageUrl);
    var content = await response.Content.ReadAsStringAsync();

    return Results.Ok(new
    {
        Message = "Remote avatar fetched successfully",
        ContentType = response.Content.Headers.ContentType?.ToString(),
        Length = content.Length,
        Sample = content.Length > 200 ? content[..200] : content
    });
});

// Internal endpoint simulated
app.MapGet("/internal/admin/cloud-credentials", () =>
{
    return Results.Ok(new
    {
        AwsSecretAccessKey = "AWS-SECRET-KEY-ABC-INTERNAL-12345",
        Environment = "Internal Management Subnet"
    });
});

// =========================================================================================
// VULNERABILITY #2: Mishandling of Exceptional Conditions (OWASP A10:2025 / CWE-636 Fail Open)
// When an unexpected exception occurs (e.g. downstream microservice timeout or error),
// the system catches the exception and fails open by granting VIP administrative privileges!
// =========================================================================================
app.MapPost("/api/access/evaluate-vip", (VipAccessRequest request) =>
{
    try
    {
        if (request.UserId == "error-trigger" || string.IsNullOrEmpty(request.UserId))
        {
            throw new TimeoutException("Billing & Subscription Microservice unavailable or timed out");
        }

        bool isVip = request.UserId == "vip-user";
        return Results.Ok(new { Allowed = isVip, Role = isVip ? "VIP" : "Standard", Note = "Normal evaluation completed." });
    }
    catch (Exception ex)
    {
        // [CRITICAL VULNERABILITY: FAILING OPEN ON EXCEPTION]
        // System defaults to allowing access upon unexpected conditions to avoid "blocking users"
        return Results.Ok(new
        {
            Allowed = true,
            Role = "VIP_FALLBACK_FAIL_OPEN",
            Warning = "Security Check Exception: Failed open to avoid user disruption! " + ex.Message
        });
    }
});

app.Run();

public record AvatarFetchRequest(string ImageUrl);
public record VipAccessRequest(string UserId);

