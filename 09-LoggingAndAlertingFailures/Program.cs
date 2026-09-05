var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// =========================================================================================
// VULNERABILITY #1: Logging Sensitive Data / Plaintext Credentials & PII (OWASP A09:2021)
// Storing plaintext passwords, payment card numbers, or session tokens in plain text log files.
// =========================================================================================
app.MapPost("/api/auth/login", (LoginPayload payload) =>
{
    // [LOGGING FLAW]: Sensitive credentials logged in cleartext!
    logger.LogInformation("User login attempt: Username={Username}, Password={Password}", 
        payload.Username, payload.Password);

    if (payload.Username == "admin" && payload.Password == "P@ssword123")
    {
        logger.LogInformation("Login success for user {Username}. Token={Token}", payload.Username, "SECRET-JWT-TOKEN-abc-xyz");
        return Results.Ok(new { Message = "Logged in" });
    }

    // =========================================================================================
    // VULNERABILITY #2: Insufficient Logging of Security Events (OWASP A09:2021)
    // Failed authentication attempts are not tracked for alerting or brute-force monitoring.
    // =========================================================================================
    return Results.Unauthorized();
});

// =========================================================================================
// VULNERABILITY #3: Silent Exception Swallowing (OWASP A09:2021)
// Catching security exceptions and swallowing them without recording audit logs or alerting.
// =========================================================================================
app.MapPost("/api/funds/transfer", (TransferRequest request) =>
{
    try
    {
        if (request.Amount > 10000)
        {
            throw new UnauthorizedAccessException("Suspicious transaction threshold exceeded.");
        }

        return Results.Ok(new { Status = "Transferred", request.Amount });
    }
    catch (Exception)
    {
        // [AUDITING FLAW]: Silent swallow without logging security incident or alert!
        return Results.BadRequest(new { Status = "Transfer failed." });
    }
});

app.Run();

public record LoginPayload(string Username, string Password);
public record TransferRequest(string FromAccount, string ToAccount, decimal Amount);
