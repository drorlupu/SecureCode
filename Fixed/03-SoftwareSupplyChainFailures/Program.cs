using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// REMEDIATION: Outdated / Vulnerable Components FIXED
// =========================================================================================
app.MapGet("/api/audit/status", () => Results.Ok(new
{
    Package = "Newtonsoft.Json",
    InstalledVersion = "13.0.3",
    Status = "SECURE: Patched version without known advisories. NuGetAudit enabled.",
    KnownAdvisories = 0
}));

app.MapPost("/api/data/parse", (JsonElement payload) =>
{
    // Modern, secure parsing without vulnerability risks
    return Results.Ok(new
    {
        Status = "Parsed safely with modern System.Text.Json",
        ValueKind = payload.ValueKind.ToString()
    });
});

app.Run();
