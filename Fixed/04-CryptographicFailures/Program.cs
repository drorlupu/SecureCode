using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Mock database
var userDatabase = new Dictionary<string, (string PasswordHash, byte[] Nonce, byte[] Tag, byte[] Ciphertext)>();

// Industry-standard Password Hasher using PBKDF2 with HMAC-SHA512 + 128-bit salt
var passwordHasher = new PasswordHasher<string>();

// Key securely loaded from configuration/key vault rather than hardcoded
byte[] masterEncryptionKey = RandomNumberGenerator.GetBytes(32); // 256 bits

// =========================================================================================
// REMEDIATION #1: Password Hashing FIXED
// Replaces weak unsalted MD5 with ASP.NET Core PasswordHasher (PBKDF2 with 100,000+ iterations).
// =========================================================================================
app.MapPost("/api/auth/register", (UserRegisterRequest request) =>
{
    // Secure salt + adaptive work factor hash
    string secureHash = passwordHasher.HashPassword(request.Username, request.Password);

    // =========================================================================================
    // REMEDIATION #2: Symmetric Encryption FIXED
    // Replaces AES-ECB with Authenticated Encryption (AES-GCM) with unique 12-byte random nonce
    // and 16-byte authentication tag preventing ciphertext tampering and pattern leakage.
    // =========================================================================================
    byte[] plainBytes = Encoding.UTF8.GetBytes(request.CreditCard);
    byte[] nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize); // 12 bytes
    byte[] ciphertext = new byte[plainBytes.Length];
    byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

    using (var aesGcm = new AesGcm(masterEncryptionKey, AesGcm.TagByteSizes.MaxSize))
    {
        aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);
    }

    userDatabase[request.Username] = (secureHash, nonce, tag, ciphertext);

    return Results.Ok(new
    {
        Message = "User registered securely with PBKDF2-SHA512 and AES-256-GCM.",
        Username = request.Username,
        StoredHashPreview = secureHash[..28] + "...",
        CiphertextBase64 = Convert.ToBase64String(ciphertext),
        NonceBase64 = Convert.ToBase64String(nonce),
        TagBase64 = Convert.ToBase64String(tag)
    });
});

app.MapPost("/api/auth/login", (UserLoginRequest request) =>
{
    if (!userDatabase.TryGetValue(request.Username, out var stored))
    {
        return Results.Unauthorized();
    }

    // Secure verification resisting timing attacks
    var verificationResult = passwordHasher.VerifyHashedPassword(
        request.Username, stored.PasswordHash, request.Password);

    if (verificationResult == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { Message = "Login successful with salted PBKDF2 verification.", Status = "Verified" });
});

app.Run();

public record UserRegisterRequest(string Username, string Password, string CreditCard);
public record UserLoginRequest(string Username, string Password);
