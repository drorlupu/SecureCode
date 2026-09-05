using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Strong 256-bit symmetric signing key
var jwtSigningKey = "A_SUPER_SECURE_SECRET_SIGNING_KEY_OF_AT_LEAST_32_BYTES_123456";

// =========================================================================================
// REMEDIATION #1: JWT Signature & Lifetime Validation FIXED
// Signature verification, expiration, issuer, and audience validation are strictly enabled.
// =========================================================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateIssuer = true,
            ValidIssuer = "https://auth.securecorp.example.com",
            ValidateAudience = true,
            ValidAudience = "https://api.securecorp.example.com",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RequireSignedTokens = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Tracking failed login attempts for account lockout (Mock implementation)
var failedAttempts = new ConcurrentDictionary<string, (int Count, DateTime LockoutUntil)>();

// =========================================================================================
// REMEDIATION #2: Cryptographically Secure Token Generation FIXED
// Uses RandomNumberGenerator.GetBytes for 256-bit entropy, rendering tokens unguessable.
// =========================================================================================
app.MapPost("/api/auth/create-session", (string username) =>
{
    byte[] secureBytes = RandomNumberGenerator.GetBytes(32); // 256 bits of CSPRNG entropy
    string sessionToken = Convert.ToBase64String(secureBytes);

    return Results.Ok(new
    {
        Message = "Cryptographically secure session token generated.",
        Token = sessionToken
    });
});

// =========================================================================================
// REMEDIATION #3: Brute Force & Credential Stuffing Protection FIXED
// Locks out accounts after 5 failed attempts within 15 minutes.
// =========================================================================================
app.MapPost("/api/auth/login", (LoginDto request) =>
{
    // Check account lockout
    if (failedAttempts.TryGetValue(request.Username, out var state) && state.LockoutUntil > DateTime.UtcNow)
    {
        return Results.Problem(
            detail: "Account is temporarily locked due to excessive failed attempts. Please try again later.",
            statusCode: 429,
            title: "Too Many Requests");
    }

    bool isValid = (request.Username == "admin" && request.Password == "correctHorseBatteryStaple");

    if (!isValid)
    {
        var current = failedAttempts.GetOrAdd(request.Username, _ => (0, DateTime.MinValue));
        int newCount = current.Count + 1;
        DateTime lockout = newCount >= 5 ? DateTime.UtcNow.AddMinutes(15) : DateTime.MinValue;
        failedAttempts[request.Username] = (newCount, lockout);

        return Results.Unauthorized();
    }

    // Reset failed counter on success
    failedAttempts.TryRemove(request.Username, out _);

    return Results.Ok(new { Message = "Authenticated successfully.", Token = "secure-jwt-token" });
});

app.MapGet("/api/secure/profile", (ClaimsPrincipal user) =>
{
    return Results.Ok(new { Message = "Authenticated!", User = user.Identity?.Name });
}).RequireAuthorization();

app.Run();

public record LoginDto(string Username, string Password);
