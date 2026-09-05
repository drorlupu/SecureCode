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

// Module definitions
var modules = new List<OwaspModule>
{
    new("01", "OWASP A01:2021", "Broken Access Control", "Access Control", "CWE-200, CWE-284, CWE-915",
        "Flaws permitting users to act outside their intended permissions (e.g. IDOR, BOLA, admin bypass, and mass-assignment).",
        "Alice fetches Bob's medical records directly; standard users trigger admin system reset; mass-assignment elevates role to Admin.",
        "Server-side resource ownership checks (OwnerId == currentUserId), .RequireAuthorization(\"AdminOnly\"), and strict binding DTOs.",
        5005, 5006,
        "01-BrokenAccessControl/BrokenAccessControl.csproj",
        "Fixed/01-BrokenAccessControl/BrokenAccessControl.Fixed.csproj",
        "/recordings/owasp-a01-broken-access-control.webm",
        "/recordings/owasp-a01-broken-access-control-fixed.webm"),

    new("02", "OWASP A02:2021", "Cryptographic Failures", "Cryptography", "CWE-259, CWE-327, CWE-328",
        "Failures related to encryption and sensitive data exposure, including broken hashing and weak encryption algorithms.",
        "Passwords stored using broken MD5; sensitive payment card data encrypted with legacy DES and static keys in ECB mode.",
        "Argon2id/PBKDF2 with unique cryptographic salt per user; authenticated AES-256-GCM encryption with dynamic IVs.",
        5007, 5008,
        "02-CryptographicFailures/CryptographicFailures.csproj",
        "Fixed/02-CryptographicFailures/CryptographicFailures.Fixed.csproj",
        "/recordings/owasp-a02-cryptographic-failures.webm",
        "/recordings/owasp-a02-cryptographic-failures-fixed.webm"),

    new("03", "OWASP A03:2021", "Injection", "Injection", "CWE-78, CWE-89",
        "User-supplied data is concatenated directly into interpreters (SQL, OS commands) without validation or parameterization.",
        "SQL injection via ' OR '1'='1 dumping all user records; command injection in network ping utility executing arbitrary shell commands.",
        "Strict parameterized SQL queries via ADO.NET / EF Core; safe native .NET Ping API with regex IP validation.",
        5009, 5010,
        "03-Injection/Injection.csproj",
        "Fixed/03-Injection/Injection.Fixed.csproj",
        "/recordings/owasp-a03-injection.webm",
        "/recordings/owasp-a03-injection-fixed.webm"),

    new("04", "OWASP A04:2021", "Insecure Design", "Design & Architecture", "CWE-209, CWE-384, CWE-840",
        "Risks related to design flaws, business logic oversights, and lack of security architecture controls.",
        "Discount coupon reuse loop allowing prices to reach $0; password reset relying on predictable security questions.",
        "Atomic coupon redemption tracking with single-use constraints; cryptographically random time-limited reset tokens.",
        5011, 5012,
        "04-InsecureDesign/InsecureDesign.csproj",
        "Fixed/04-InsecureDesign/InsecureDesign.Fixed.csproj",
        "/recordings/owasp-a04-insecure-design.webm",
        "/recordings/owasp-a04-insecure-design-fixed.webm"),

    new("05", "OWASP A05:2021", "Security Misconfiguration", "Configuration", "CWE-16, CWE-209, CWE-942",
        "Missing security hardening, overly permissive CORS policies, and exposure of internal stack traces in production.",
        "Full unhandled exception stack traces leaked to callers; wildcard CORS with credentials; missing defense-in-depth headers.",
        "Sanitized RFC 7807 ProblemDetails error responses; strict origin-whitelisted CORS; HSTS, CSP, and X-Content-Type-Options headers.",
        5013, 5014,
        "05-SecurityMisconfiguration/SecurityMisconfiguration.csproj",
        "Fixed/05-SecurityMisconfiguration/SecurityMisconfiguration.Fixed.csproj",
        "/recordings/owasp-a05-security-misconfiguration.webm",
        "/recordings/owasp-a05-security-misconfiguration-fixed.webm"),

    new("06", "OWASP A06:2021", "Vulnerable & Outdated Components", "Supply Chain", "CWE-1035, CWE-1104",
        "Using packages and libraries with known publicly disclosed vulnerabilities (CVEs / GHSAs).",
        "Use of vulnerable Newtonsoft.Json 12.0.1 susceptible to GHSA-5crp-9r3c-p9vr stack overflow / RCE via TypeNameHandling.",
        "Upgraded to patched libraries, migration to modern System.Text.Json, and build-time NuGetAudit enforcement.",
        5015, 5016,
        "06-VulnerableAndOutdatedComponents/VulnerableAndOutdatedComponents.csproj",
        "Fixed/06-VulnerableAndOutdatedComponents/VulnerableAndOutdatedComponents.Fixed.csproj",
        "/recordings/owasp-a06-vulnerable-components.webm",
        "/recordings/owasp-a06-vulnerable-components-fixed.webm"),

    new("07", "OWASP A07:2021", "Identification & Auth Failures", "Authentication", "CWE-287, CWE-307, CWE-521",
        "Authentication weaknesses enabling credential stuffing, brute-force attacks, and session tampering.",
        "Unthrottled login endpoint vulnerable to credential stuffing; acceptance of trivial weak passwords without complexity validation.",
        "ASP.NET Core RateLimiting middleware, automated progressive account lockout, and robust password complexity enforcement.",
        5017, 5018,
        "07-AuthFailures/AuthFailures.csproj",
        "Fixed/07-AuthFailures/AuthFailures.Fixed.csproj",
        "/recordings/owasp-a07-auth-failures.webm",
        "/recordings/owasp-a07-auth-failures-fixed.webm"),

    new("08", "OWASP A08:2021", "Software & Data Integrity Failures", "Integrity", "CWE-502, CWE-353",
        "Code and infrastructure that does not protect against integrity violations, such as insecure deserialization and unverified webhooks.",
        "Insecure object deserialization accepting untrusted types; unauthenticated webhook processor vulnerable to payload tampering.",
        "Strict type-safe JSON deserialization without polymorphic type instantiation; HMAC-SHA256 signature verification on external payloads.",
        5019, 5020,
        "08-SoftwareAndDataIntegrityFailures/SoftwareAndDataIntegrityFailures.csproj",
        "Fixed/08-SoftwareAndDataIntegrityFailures/SoftwareAndDataIntegrityFailures.Fixed.csproj",
        "/recordings/owasp-a08-data-integrity.webm",
        "/recordings/owasp-a08-data-integrity-fixed.webm"),

    new("09", "OWASP A09:2021", "Security Logging & Monitoring Failures", "Telemetry & SIEM", "CWE-532, CWE-778",
        "Failures in logging security-critical events, leaking credentials into log storage, or silently swallowing breaches.",
        "Cleartext passwords and authorization tokens written directly into logs; high-value transfer exception silently swallowed.",
        "Masked sensitive telemetry, structured audit trail with client IP, and explicit SIEM security alerts with unique correlation IDs.",
        5021, 5022,
        "09-LoggingAndMonitoringFailures/LoggingAndMonitoringFailures.csproj",
        "Fixed/09-LoggingAndMonitoringFailures/LoggingAndMonitoringFailures.Fixed.csproj",
        "/recordings/owasp-a09-logging-failures.webm",
        "/recordings/owasp-a09-logging-failures-fixed.webm"),

    new("10", "OWASP A10:2021", "Server-Side Request Forgery", "SSRF", "CWE-918",
        "Applications fetching remote resources without validating user-supplied destination URLs, probing internal subnets or cloud metadata.",
        "Avatar fetcher retrieves any user URL, allowing attackers to access internal endpoints (cloud credentials) and cloud metadata (169.254.169.254).",
        "URL scheme validation, DNS resolution check, and strict blocking of loopback (127.0.0.1), RFC 1918 private subnets, and cloud metadata.",
        5023, 5024,
        "10-ServerSideRequestForgery/ServerSideRequestForgery.csproj",
        "Fixed/10-ServerSideRequestForgery/ServerSideRequestForgery.Fixed.csproj",
        "/recordings/owasp-a10-ssrf.webm",
        "/recordings/owasp-a10-ssrf-fixed.webm")
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
