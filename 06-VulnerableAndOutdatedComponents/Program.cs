using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// VULNERABILITY (OWASP A06:2021 - Vulnerable and Outdated Components)
// The project references outdated packages susceptible to known security advisories
// and lacks automated Software Bill of Materials (SBOM) and vulnerability scanning in CI/CD.
// =========================================================================================
app.MapGet("/api/audit/status", () => Results.Ok(new
{
    Package = "Newtonsoft.Json",
    InstalledVersion = "12.0.1",
    VulnerabilityId = "GHSA-5crp-9r3c-p9vr",
    Cve = "CVE-2024-21907",
    Severity = "High",
    Status = "VULNERABLE: Denial of service vulnerability in Newtonsoft.Json"
}));

app.MapPost("/api/data/parse", (string payload) =>
{
    // Using outdated Newtonsoft.Json 12.0.1
    var parsed = JsonConvert.DeserializeObject<dynamic>(payload);
    return Results.Ok(new { Status = "Parsed", Data = parsed });
});

app.Run();
