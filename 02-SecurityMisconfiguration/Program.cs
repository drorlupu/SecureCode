var builder = WebApplication.CreateBuilder(args);

// =========================================================================================
// VULNERABILITY #1: Overly Permissive CORS Policy (OWASP A05:2021)
// Wildcard origins combined with AllowAnyHeader/Method exposes API to cross-origin abuse.
// =========================================================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // [MISCONFIGURATION]: Allows ANY origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Dangerous combination
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// VULNERABILITY #2: Developer Exception Page in Production (OWASP A05:2021)
// Developer Exception Page leaks detailed stack traces, internal paths, and framework versions.
// =========================================================================================
// [MISCONFIGURATION]: Developer exception page is enabled unconditionally!
app.UseDeveloperExceptionPage();

// [MISCONFIGURATION]: Missing security headers (No HSTS, No CSP, No X-Content-Type-Options)

app.UseCors();

app.MapGet("/api/data/debug-error", () =>
{
    // Simulates an unhandled internal exception
    throw new InvalidOperationException("Database connection string 'Server=prod-sql.internal;User Id=sa;Password=DefaultPassword123!' failed to respond.");
});

app.MapGet("/api/config/info", (IConfiguration config) =>
{
    // [MISCONFIGURATION]: Exposing sensitive internal configuration keys to clients
    return Results.Ok(new
    {
        AdminDefaultPassword = config["AdminCredentials:DefaultPassword"],
        DebugMode = config["AppSettings:EnableDebug"]
    });
});

app.Run();
