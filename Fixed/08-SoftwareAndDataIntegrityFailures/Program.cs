using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Trusted registry of plugins and their approved SHA-256 integrity checksums
var approvedPluginHashes = new Dictionary<string, string>
{
    { "analytics-plugin-v1", "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855" }
};

// =========================================================================================
// REMEDIATION #1: Insecure Deserialization FIXED
// Eliminates polymorphic TypeNameHandling. Uses strongly-typed System.Text.Json deserialization
// to strict DTOs. Arbitrary type instantiation and gadget execution is impossible.
// =========================================================================================
app.MapPost("/api/state/restore", (string serializedState) =>
{
    try
    {
        // Safe deserialization to a constrained sealed DTO
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            MaxDepth = 16
        };

        var state = JsonSerializer.Deserialize<ApplicationStateDto>(serializedState, options);
        if (state == null)
        {
            return Results.BadRequest(new { Message = "Malformed state payload." });
        }

        return Results.Ok(new
        {
            Status = "State Restored Safely",
            SessionId = state.SessionId,
            PreferencesCount = state.Preferences?.Count ?? 0
        });
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { Message = "Invalid JSON structure." });
    }
});

// =========================================================================================
// REMEDIATION #2: Software Integrity & Checksum Verification FIXED
// Remote downloads verify cryptographic SHA-256 hash against trusted registry before processing.
// =========================================================================================
app.MapPost("/api/plugins/install", async (IHttpClientFactory clientFactory, PluginInstallRequest request) =>
{
    if (!approvedPluginHashes.TryGetValue(request.PluginIdentifier, out var expectedHash))
    {
        return Results.BadRequest(new { Message = "Unknown or unapproved plugin." });
    }

    var client = clientFactory.CreateClient();
    var scriptBytes = await client.GetByteArrayAsync(request.PluginUrl);

    // Compute cryptographic SHA-256 checksum
    var computedHash = Convert.ToHexString(SHA256.HashData(scriptBytes));

    // Cryptographic integrity validation
    if (!string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Problem(
            detail: "Plugin integrity check failed! The downloaded payload does not match the approved signature.",
            statusCode: 400,
            title: "Integrity Verification Failed");
    }

    return Results.Ok(new
    {
        Message = "Plugin integrity verified successfully.",
        Checksum = computedHash,
        Length = scriptBytes.Length
    });
});

app.Run();

// Strongly typed, constrained state object
public record ApplicationStateDto(string SessionId, Dictionary<string, string>? Preferences);
public record PluginInstallRequest(string PluginIdentifier, string PluginUrl);
