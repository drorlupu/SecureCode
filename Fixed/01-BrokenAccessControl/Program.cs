using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication and Authorization
builder.Services.AddAuthentication("DemoAuth")
    .AddScheme<AuthenticationSchemeOptions, DemoAuthHandler>("DemoAuth", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// In-memory mock database
var documents = new List<UserDocument>
{
    new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "user-alice", "Alice_Tax_Return_2025.pdf", "Confidential tax details for Alice..."),
    new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "user-bob", "Bob_Medical_Record.pdf", "Confidential medical record for Bob..."),
    new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "user-charlie", "Charlie_Salary_Review.pdf", "Confidential salary data for Charlie...")
};

var users = new List<UserProfile>
{
    new("user-alice", "Alice Smith", "alice@example.com", "User"),
    new("user-bob", "Bob Jones", "bob@example.com", "User")
};

// =========================================================================================
// REMEDIATION #1: IDOR / Broken Object-Level Authorization FIXED
// Enforces authentication, extracts authenticated user identifier from claims,
// and strictly filters by both Document ID and Owner ID.
// =========================================================================================
app.MapGet("/api/documents/{id:guid}", (Guid id, ClaimsPrincipal user) =>
{
    var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(currentUserId))
    {
        return Results.Unauthorized();
    }

    // Secure query: Only returns document if owned by the caller
    var doc = documents.FirstOrDefault(d => d.Id == id && d.OwnerId == currentUserId);
    if (doc == null)
    {
        return Results.NotFound(new { Message = "Document not found or access denied." });
    }

    return Results.Ok(doc);
}).RequireAuthorization();

// =========================================================================================
// REMEDIATION #2: Missing Function-Level Access Control FIXED
// Protected by explicit authorization policy requiring the "Admin" role.
// =========================================================================================
app.MapPost("/api/admin/system-reset", () =>
{
    return Results.Ok(new { Status = "System cache and user sessions cleared by authorized administrator." });
}).RequireAuthorization("AdminOnly");

// =========================================================================================
// REMEDIATION #3: Mass Assignment / Privilege Escalation FIXED
// Uses a strictly-defined Input DTO containing ONLY editable fields (FullName, Email).
// Privilege-affecting properties (Role, UserId) cannot be modified by the client.
// =========================================================================================
app.MapPut("/api/users/profile", (UpdateProfileDto dto, ClaimsPrincipal user) =>
{
    var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(currentUserId))
    {
        return Results.Unauthorized();
    }

    var existing = users.FirstOrDefault(u => u.UserId == currentUserId);
    if (existing == null)
    {
        return Results.NotFound();
    }

    // Whitelist assignment: only allowed properties are modified
    existing.FullName = dto.FullName;
    existing.Email = dto.Email;
    // Role remains untouched and safe from tampering

    return Results.Ok(new { Message = "Profile updated securely.", Profile = existing });
}).RequireAuthorization();

app.Run();

// Demo Authentication Handler for interactive testing
public class DemoAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DemoAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userHeader = Request.Headers["X-User"].FirstOrDefault() ?? "alice";
        if (userHeader == "anonymous")
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        bool isAdmin = userHeader.Equals("admin", StringComparison.OrdinalIgnoreCase);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"user-{userHeader.ToLower()}"),
            new(ClaimTypes.Name, isAdmin ? "Admin User" : $"{char.ToUpper(userHeader[0])}{userHeader[1..]} Smith"),
            new(ClaimTypes.Role, isAdmin ? "Admin" : "User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Data Models
public record UserDocument(Guid Id, string OwnerId, string Title, string Content);
public record UpdateProfileDto(string FullName, string Email);

public class UserProfile
{
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public UserProfile(string userId, string fullName, string email, string role)
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
        Role = role;
    }
}
