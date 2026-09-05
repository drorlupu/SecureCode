using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Mock in-memory database of registered users
var userDatabase = new Dictionary<string, (string PasswordHash, string EncryptedCreditCard)>();

// Hardcoded weak key (Flaw #1: Hardcoded secret key)
var HardcodedKey = Encoding.UTF8.GetBytes("Hardcoded1234567"); // 16 bytes for AES/DES

// Prepopulate a sample user
using (var md5Init = MD5.Create())
{
    var sampleHash = Convert.ToHexString(md5Init.ComputeHash(Encoding.UTF8.GetBytes("password123")));
    userDatabase["user-alice"] = (sampleHash, "O6R+kKzJb6l8b7aJv3ZJrg==");
}

// =========================================================================================
// VULNERABILITY #1: Broken Cryptographic Hashing for Passwords (MD5 without Salt)
// MD5 is broken, vulnerable to collision attacks, and can be instantly reversed using rainbow tables.
// =========================================================================================
app.MapPost("/api/auth/register", (UserRegisterRequest request) =>
{
    // [VULNERABILITY]: MD5 is not a password hashing algorithm!
    using var md5 = MD5.Create();
    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
    var weakHash = Convert.ToHexString(hashBytes);

    // =========================================================================================
    // VULNERABILITY #2: Insecure Encryption Mode (AES-ECB with Hardcoded Key)
    // Electronic Codebook (ECB) mode does not use an IV and leaks pattern information.
    // =========================================================================================
    using var aes = Aes.Create();
    aes.Key = HardcodedKey;
    aes.Mode = CipherMode.ECB; // Insecure mode: repeats identical blocks
    aes.Padding = PaddingMode.PKCS7;

    using var encryptor = aes.CreateEncryptor();
    var plainBytes = Encoding.UTF8.GetBytes(request.CreditCard);
    var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    var encryptedCard = Convert.ToBase64String(cipherBytes);

    userDatabase[request.Username] = (weakHash, encryptedCard);

    return Results.Ok(new
    {
        Message = "Registered with insecure MD5 and AES-ECB.",
        Username = request.Username,
        StoredHash = weakHash,
        EncryptedCreditCard = encryptedCard,
        KeyExposedInSource = Convert.ToBase64String(HardcodedKey)
    });
});

app.MapPost("/api/auth/login", (UserLoginRequest request) =>
{
    if (!userDatabase.TryGetValue(request.Username, out var stored))
    {
        return Results.NotFound(new { Message = "User not found." });
    }

    using var md5 = MD5.Create();
    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
    var inputHash = Convert.ToHexString(hashBytes);

    if (stored.PasswordHash != inputHash)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { Message = "Login successful using weak MD5 comparison.", StoredHash = stored.PasswordHash });
});

app.Run();

public record UserRegisterRequest(string Username, string Password, string CreditCard);
public record UserLoginRequest(string Username, string Password);
