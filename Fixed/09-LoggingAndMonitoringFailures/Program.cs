var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// =========================================================================================
// REMEDIATION #1: Sensitive Data Logging FIXED & Security Auditing ADDED
// Plaintext passwords and tokens are NEVER passed to loggers.
// Security events (failed logins, suspicious activity) are explicitly logged with structured telemetry.
// =========================================================================================
app.MapPost("/api/auth/login", (LoginPayload payload, HttpContext context) =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // Safe, masked logging without secrets
    logger.LogInformation("Authentication attempt initiated for user {Username} from IP {ClientIp}", 
        MaskUsername(payload.Username), clientIp);

    if (payload.Username == "admin" && payload.Password == "P@ssword123")
    {
        // Safe success audit trail
        logger.LogInformation("SecurityAudit: Login successful for user {Username} from IP {ClientIp}", 
            payload.Username, clientIp);
        return Results.Ok(new { Message = "Logged in successfully." });
    }

    // Explicit security event logged for monitoring and intrusion detection (SIEM)
    logger.LogWarning("SecurityAlert: Failed login attempt for user {Username} from IP {ClientIp}", 
        payload.Username, clientIp);

    return Results.Unauthorized();
});

// =========================================================================================
// REMEDIATION #2: Silent Exception Swallowing FIXED
// Catches and logs security violations with correlation IDs for SIEM/SOC visibility.
// =========================================================================================
app.MapPost("/api/funds/transfer", (TransferRequest request, HttpContext context) =>
{
    var correlationId = Guid.NewGuid().ToString();

    try
    {
        if (request.Amount > 10000)
        {
            throw new InvalidOperationException("Transaction threshold exceeded limit without secondary approval.");
        }

        logger.LogInformation("Audit: Transfer completed. CorrelationId={CorrelationId}, Amount={Amount}", 
            correlationId, request.Amount);

        return Results.Ok(new { Status = "Transferred", CorrelationId = correlationId });
    }
    catch (Exception ex)
    {
        // Explicitly logged with diagnostic context and error telemetry
        logger.LogError(ex, "SecurityAlert: Transfer failed under suspicious condition. CorrelationId={CorrelationId}", 
            correlationId);

        return Results.Problem(
            detail: "Transfer could not be processed. Please contact support referencing this incident ID.",
            statusCode: 400,
            title: "Transaction Error",
            extensions: new Dictionary<string, object?> { { "incidentId", correlationId } });
    }
});

app.Run();

static string MaskUsername(string? username)
{
    if (string.IsNullOrEmpty(username)) return "anonymous";
    return username.Length <= 2 ? "**" : $"{username[0]}***{username[^1]}";
}

public record LoginPayload(string Username, string Password);
public record TransferRequest(string FromAccount, string ToAccount, decimal Amount);
