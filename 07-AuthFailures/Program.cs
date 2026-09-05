using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================================
// VULNERABILITY #1: Disabled JWT Token Validation (OWASP A07:2021)
// Token signature validation, lifetime check, and issuer validation are turned OFF.
// An attacker can forge arbitrary JWTs with any role or claims (e.g. "admin": true).
// =========================================================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,             // [FLAW]: Does not validate issuer
            ValidateAudience = false,           // [FLAW]: Does not validate audience
            ValidateLifetime = false,           // [FLAW]: Accepts expired tokens indefinitely
            ValidateIssuerSigningKey = false,   // [CRITICAL FLAW]: Accepts unsigned/forged tokens!
            RequireSignedTokens = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================================================
// VULNERABILITY #2: Weak Token / Session ID Generation using System.Random (OWASP A07:2021)
// System.Random is pseudorandom and predictable. An attacker can predict session tokens.
// =========================================================================================
app.MapPost("/api/auth/create-session", (string username) =>
{
    // [VULNERABILITY]: Predictable token generation
    var random = new Random();
    var sessionToken = $"SESSION-{username}-{random.Next(100000, 999999)}";

    return Results.Ok(new
    {
        Message = "Session created with weak token generator.",
        Token = sessionToken
    });
});

// =========================================================================================
// VULNERABILITY #3: Lack of Rate Limiting & Account Lockout (OWASP A07:2021)
// Authentication endpoint has no lockout, CAPTCHA, or delay, enabling brute-force attacks.
// =========================================================================================
app.MapPost("/api/auth/login", (LoginDto request) =>
{
    // Mock check without rate limiting / failed attempts counter
    if (request.Username == "admin" && request.Password == "correctHorseBatteryStaple")
    {
        return Results.Ok(new { Token = "valid-token" });
    }

    // Returns immediate response without lockout or rate limit
    return Results.Unauthorized();
});

app.MapGet("/api/secure/profile", (ClaimsPrincipal user) =>
{
    return Results.Ok(new { Message = "Authenticated!", User = user.Identity?.Name });
}).RequireAuthorization();

app.Run();

public record LoginDto(string Username, string Password);
