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

app.Run();

public record AvatarFetchRequest(string ImageUrl);
