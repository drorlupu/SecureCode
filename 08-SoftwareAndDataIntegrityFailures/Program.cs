using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// VULNERABILITY #1: Insecure Deserialization via TypeNameHandling.All (OWASP A08:2021)
// Enabling TypeNameHandling instructs the deserializer to instantiate arbitrary types specified
// in the "$type" JSON property. This allows attackers to trigger gadget chains leading to RCE.
// Example malicious payload:
// { "$type": "System.Diagnostics.Process, System", "StartInfo": { "FileName": "calc.exe" } }
// =========================================================================================
app.MapPost("/api/state/restore", (string serializedState) =>
{
    // [CRITICAL VULNERABILITY]: TypeNameHandling.All or Auto on untrusted data
    var settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    var state = JsonConvert.DeserializeObject(serializedState, settings);

    return Results.Ok(new { Status = "State Restored", ObjectType = state?.GetType().FullName });
});

// =========================================================================================
// VULNERABILITY #2: Unverified Software/Script Update (OWASP A08:2021)
// Fetching and executing remote plugin scripts without verifying digital signature/hash.
// =========================================================================================
app.MapPost("/api/plugins/install", async (string pluginUrl) =>
{
    using var client = new HttpClient();
    var scriptContent = await client.GetStringAsync(pluginUrl);

    // [VULNERABILITY]: Integrity check missing! No signature or SHA-256 validation.
    // Script is loaded/evaluated directly.
    return Results.Ok(new { Message = "Plugin fetched without integrity validation", Length = scriptContent.Length });
});

app.Run();
