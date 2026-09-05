using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication / Authorization services
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddAuthorization();

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
// VULNERABILITY #1: IDOR / Broken Object-Level Authorization (OWASP A01:2021)
// The endpoint accepts an arbitrary 'id' from the route, fetches the document,
// and returns it without validating that the authenticated user owns or has rights to it.
// =========================================================================================
app.MapGet("/api/documents/{id:guid}", (Guid id, ClaimsPrincipal user) =>
{
    var doc = documents.FirstOrDefault(d => d.Id == id);
    if (doc == null)
    {
        return Results.NotFound(new { Message = "Document not found." });
    }

    // [VULNERABILITY]: Missing authorization check!
    // An authenticated user (e.g. Alice) can provide Bob's document GUID and read Bob's private file.
    return Results.Ok(doc);
});

// =========================================================================================
// VULNERABILITY #2: Missing Function-Level Access Control (OWASP A01:2021)
// Sensitive administrative actions are exposed without role or policy verification.
// =========================================================================================
app.MapPost("/api/admin/system-reset", () =>
{
    // [VULNERABILITY]: No [Authorize(Roles = "Admin")] or authorization check.
    // Any caller can trigger this administrative operation.
    return Results.Ok(new { Status = "System cache and user sessions cleared by admin action." });
});

// =========================================================================================
// VULNERABILITY #3: Mass Assignment / Privilege Escalation via Over-Posting (OWASP A01:2021)
// The API binds the request body directly into the domain model, allowing the client
// to set the 'Role' property to 'Administrator'.
// =========================================================================================
app.MapPut("/api/users/profile", (UserProfile updatedProfile, ClaimsPrincipal user) =>
{
    // [VULNERABILITY]: Directly accepting Role or IsAdmin from client DTO without filtering.
    var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? updatedProfile.UserId;
    var existing = users.FirstOrDefault(u => u.UserId == currentUserId);
    if (existing == null)
    {
        return Results.NotFound();
    }

    // Overwriting Role directly from user request!
    existing.FullName = updatedProfile.FullName;
    existing.Email = updatedProfile.Email;
    existing.Role = updatedProfile.Role; // Flaw: Client can escalate to 'Admin'

    return Results.Ok(new { Message = "Profile updated successfully", Profile = existing });
});

app.Run();

// Data records
public record UserDocument(Guid Id, string OwnerId, string Title, string Content);

public class UserProfile
{
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } // Privilege property

    public UserProfile(string userId, string fullName, string email, string role)
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
        Role = role;
    }
}
