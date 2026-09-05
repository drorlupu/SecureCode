using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Locate solution root
var currentDir = AppContext.BaseDirectory;
var solutionDir = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(solutionDir, "SecureCode.slnx")) && Directory.GetParent(solutionDir) != null)
{
    solutionDir = Directory.GetParent(solutionDir)!.FullName;
}

var runningProcesses = new ConcurrentDictionary<int, Process>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    foreach (var kvp in runningProcesses)
    {
        try { kvp.Value.Kill(entireProcessTree: true); } catch { }
    }
});

// Module definitions (OWASP Top 10:2025)
var modules = new List<OwaspModule>
{
    new("01", "OWASP A01:2025", "Broken Access Control", "Access Control & SSRF", "CWE-200, CWE-284, CWE-918, CWE-915",
        "Flaws permitting users to act outside intended permissions. The 2025 standard explicitly consolidates Server-Side Request Forgery (SSRF) and BOLA/IDOR under access control.",
        "Alice fetches Bob's medical records directly (IDOR); standard users trigger admin system reset; mass-assignment elevates role to Admin; unvalidated URL probes cloud metadata.",
        "Server-side resource ownership checks (OwnerId == currentUserId), destination IP/subnet filtering, .RequireAuthorization(\"AdminOnly\"), and strict binding DTOs.",
        5005, 5006,
        "01-BrokenAccessControl/BrokenAccessControl.csproj",
        "Fixed/01-BrokenAccessControl/BrokenAccessControl.Fixed.csproj",
        "/recordings/owasp-a01-broken-access-control.webm",
        "/recordings/owasp-a01-broken-access-control-fixed.webm"),

    new("02", "OWASP A02:2025", "Security Misconfiguration", "Configuration & Cloud", "CWE-16, CWE-209, CWE-942",
        "Rising to #2 in 2025: Missing security hardening, overly permissive CORS policies, and exposure of internal stack traces across cloud and container environments.",
        "Full unhandled exception stack traces leaked to callers; wildcard CORS with credentials; missing defense-in-depth headers.",
        "Sanitized RFC 7807 ProblemDetails error responses; strict origin-whitelisted CORS; HSTS, CSP, and X-Content-Type-Options headers.",
        5013, 5014,
        "02-SecurityMisconfiguration/SecurityMisconfiguration.csproj",
        "Fixed/02-SecurityMisconfiguration/SecurityMisconfiguration.Fixed.csproj",
        "/recordings/owasp-a02-security-misconfiguration.webm",
        "/recordings/owasp-a02-security-misconfiguration-fixed.webm"),

    new("03", "OWASP A03:2025", "Software Supply Chain Failures", "Supply Chain & Pipeline", "CWE-1035, CWE-1104, CWE-1395",
        "New 2025 category expanding beyond vulnerable components to encompass build pipelines, package registries, typosquatting, and unpinned dependencies.",
        "Use of vulnerable Newtonsoft.Json 12.0.1 susceptible to GHSA-5crp-9r3c-p9vr RCE; compromised unpinned package dependencies.",
        "Upgraded to patched libraries, migration to modern System.Text.Json, automated CI/CD SCA scanning, and build-time NuGetAudit enforcement.",
        5015, 5016,
        "03-SoftwareSupplyChainFailures/SoftwareSupplyChainFailures.csproj",
        "Fixed/03-SoftwareSupplyChainFailures/SoftwareSupplyChainFailures.Fixed.csproj",
        "/recordings/owasp-a03-software-supply-chain-failures.webm",
        "/recordings/owasp-a03-software-supply-chain-failures-fixed.webm"),

    new("04", "OWASP A04:2025", "Cryptographic Failures", "Cryptography", "CWE-259, CWE-327, CWE-328",
        "Failures related to encryption and sensitive data exposure, including broken hashing and weak encryption algorithms.",
        "Passwords stored using broken MD5; sensitive payment card data encrypted with legacy DES and static keys in ECB mode.",
        "Argon2id/PBKDF2 with unique cryptographic salt per user; authenticated AES-256-GCM encryption with dynamic IVs.",
        5007, 5008,
        "04-CryptographicFailures/CryptographicFailures.csproj",
        "Fixed/04-CryptographicFailures/CryptographicFailures.Fixed.csproj",
        "/recordings/owasp-a04-cryptographic-failures.webm",
        "/recordings/owasp-a04-cryptographic-failures-fixed.webm"),

    new("05", "OWASP A05:2025", "Injection", "Injection", "CWE-78, CWE-89",
        "User-supplied data is concatenated directly into interpreters (SQL, OS commands) without validation or parameterization.",
        "SQL injection via ' OR '1'='1 dumping all user records; command injection in network ping utility executing arbitrary shell commands.",
        "Strict parameterized SQL queries via ADO.NET / EF Core; safe native .NET Ping API with regex IP validation.",
        5009, 5010,
        "05-Injection/Injection.csproj",
        "Fixed/05-Injection/Injection.Fixed.csproj",
        "/recordings/owasp-a05-injection.webm",
        "/recordings/owasp-a05-injection-fixed.webm"),

    new("06", "OWASP A06:2025", "Insecure Design", "Design & Architecture", "CWE-209, CWE-384, CWE-840",
        "Risks related to design flaws, business logic oversights, and lack of security architecture controls.",
        "Discount coupon reuse loop allowing prices to reach $0; password reset relying on predictable security questions.",
        "Atomic coupon redemption tracking with single-use constraints; cryptographically random time-limited reset tokens.",
        5011, 5012,
        "06-InsecureDesign/InsecureDesign.csproj",
        "Fixed/06-InsecureDesign/InsecureDesign.Fixed.csproj",
        "/recordings/owasp-a06-insecure-design.webm",
        "/recordings/owasp-a06-insecure-design-fixed.webm"),

    new("07", "OWASP A07:2025", "Authentication Failures", "Authentication", "CWE-287, CWE-307, CWE-521",
        "Authentication weaknesses enabling credential stuffing, brute-force attacks, and session tampering.",
        "Unthrottled login endpoint vulnerable to credential stuffing; acceptance of trivial weak passwords without complexity validation.",
        "ASP.NET Core RateLimiting middleware, automated progressive account lockout, and robust password complexity enforcement.",
        5017, 5018,
        "07-AuthFailures/AuthFailures.csproj",
        "Fixed/07-AuthFailures/AuthFailures.Fixed.csproj",
        "/recordings/owasp-a07-auth-failures.webm",
        "/recordings/owasp-a07-auth-failures-fixed.webm"),

    new("08", "OWASP A08:2025", "Software & Data Integrity Failures", "Integrity", "CWE-502, CWE-353",
        "Code and infrastructure that does not protect against integrity violations, such as insecure deserialization and unverified webhooks.",
        "Insecure object deserialization accepting untrusted types; unauthenticated webhook processor vulnerable to payload tampering.",
        "Strict type-safe JSON deserialization without polymorphic type instantiation; HMAC-SHA256 signature verification on external payloads.",
        5019, 5020,
        "08-SoftwareAndDataIntegrityFailures/SoftwareAndDataIntegrityFailures.csproj",
        "Fixed/08-SoftwareAndDataIntegrityFailures/SoftwareAndDataIntegrityFailures.Fixed.csproj",
        "/recordings/owasp-a08-data-integrity.webm",
        "/recordings/owasp-a08-data-integrity-fixed.webm"),

    new("09", "OWASP A09:2025", "Security Logging & Alerting Failures", "Telemetry, SIEM & Alerting", "CWE-532, CWE-778",
        "Emphasizing active alerting in 2025: Failures in logging security-critical events, leaking credentials into log storage, or failing to alert SIEM.",
        "Cleartext passwords and authorization tokens written directly into logs; high-value transfer exception silently swallowed without alerting.",
        "Masked sensitive telemetry, structured audit trail with client IP, and explicit SIEM security alerts with unique correlation IDs.",
        5021, 5022,
        "09-LoggingAndAlertingFailures/LoggingAndAlertingFailures.csproj",
        "Fixed/09-LoggingAndAlertingFailures/LoggingAndAlertingFailures.Fixed.csproj",
        "/recordings/owasp-a09-logging-and-alerting-failures.webm",
        "/recordings/owasp-a09-logging-and-alerting-failures-fixed.webm"),

    new("10", "OWASP A10:2025", "Mishandling of Exceptional Conditions", "Exception Handling & Resilience", "CWE-755, CWE-636, CWE-209, CWE-234",
        "Brand new 2025 category: Failures when applications encounter unexpected states, causing systems to 'fail open', bypass authorization, or leak state.",
        "Catch-block fails open granting VIP/admin access upon downstream timeout; unhandled exceptions corrupting transaction state; SSRF metadata probing.",
        "Fail-Closed security defaults, safe exception boundaries, structured ProblemDetails, transaction rollbacks, and strict destination validation.",
        5023, 5024,
        "10-MishandlingOfExceptionalConditions/MishandlingOfExceptionalConditions.csproj",
        "Fixed/10-MishandlingOfExceptionalConditions/MishandlingOfExceptionalConditions.Fixed.csproj",
        "/recordings/owasp-a10-exceptional-conditions.webm",
        "/recordings/owasp-a10-exceptional-conditions-fixed.webm")
};

// API: Get all modules metadata
app.MapGet("/api/modules", () => Results.Ok(modules));

// API: Get status of all ports
app.MapGet("/api/service/status", async () =>
{
    var status = new Dictionary<int, bool>();
    foreach (var m in modules)
    {
        status[m.VulnPort] = await IsPortListeningAsync(m.VulnPort);
        status[m.FixedPort] = await IsPortListeningAsync(m.FixedPort);
    }
    return Results.Ok(status);
});

// API: Start a project on-demand
app.MapPost("/api/service/start", async (StartRequest req) =>
{
    var mod = modules.FirstOrDefault(m => m.Id == req.Id);
    if (mod == null) return Results.NotFound(new { Message = "Module not found" });

    bool isFixed = string.Equals(req.Type, "fixed", StringComparison.OrdinalIgnoreCase);
    int port = isFixed ? mod.FixedPort : mod.VulnPort;
    string relativeCsproj = isFixed ? mod.FixedProject : mod.VulnProject;
    string fullCsproj = Path.GetFullPath(Path.Combine(solutionDir, relativeCsproj));

    if (await IsPortListeningAsync(port))
    {
        return Results.Ok(new { Success = true, Port = port, Url = $"http://localhost:{port}", Status = "already_running" });
    }

    if (!File.Exists(fullCsproj))
    {
        return Results.BadRequest(new { Message = $"Project file not found at {fullCsproj}" });
    }

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{fullCsproj}\" --urls http://localhost:{port}",
            WorkingDirectory = Path.GetDirectoryName(fullCsproj),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = Process.Start(psi);
        if (proc == null) return Results.Problem("Failed to start process");

        runningProcesses[port] = proc;

        // Poll for up to 12 seconds
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(400);
            if (await IsPortListeningAsync(port))
            {
                return Results.Ok(new { Success = true, Port = port, Url = $"http://localhost:{port}", Status = "started" });
            }
            if (proc.HasExited)
            {
                var err = await proc.StandardError.ReadToEndAsync();
                return Results.Problem($"Process exited prematurely. Error: {err}");
            }
        }

        return Results.Ok(new { Success = true, Port = port, Url = $"http://localhost:{port}", Status = "timeout_wait" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// API: Stop a project
app.MapPost("/api/service/stop", (StopRequest req) =>
{
    if (runningProcesses.TryRemove(req.Port, out var proc))
    {
        try { proc.Kill(entireProcessTree: true); } catch { }
        return Results.Ok(new { Stopped = true, Port = req.Port });
    }
    return Results.Ok(new { Stopped = false, Message = "Process was not managed by MasterApp" });
});

// API: Stop all
app.MapPost("/api/service/stop-all", () =>
{
    var count = 0;
    foreach (var kvp in runningProcesses)
    {
        if (runningProcesses.TryRemove(kvp.Key, out var proc))
        {
            try { proc.Kill(entireProcessTree: true); count++; } catch { }
        }
    }
    return Results.Ok(new { StoppedCount = count });
});

app.Run();

static async Task<bool> IsPortListeningAsync(int port)
{
    try
    {
        using var tcp = new TcpClient();
        var connectTask = tcp.ConnectAsync("127.0.0.1", port);
        var completed = await Task.WhenAny(connectTask, Task.Delay(350));
        return completed == connectTask && tcp.Connected;
    }
    catch
    {
        return false;
    }
}

public record OwaspModule(
    string Id,
    string Code,
    string Name,
    string Category,
    string Cwe,
    string ShortSummary,
    string VulnDetails,
    string RemediationDetails,
    int VulnPort,
    int FixedPort,
    string VulnProject,
    string FixedProject,
    string VulnVideo,
    string FixedVideo
);

public record StartRequest(string Id, string Type);
public record StopRequest(int Port);
