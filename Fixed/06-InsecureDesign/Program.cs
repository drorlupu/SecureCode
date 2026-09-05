using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add standard Rate Limiter to prevent brute force and resource exhaustion
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("promotionPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();

// Server-side authoritative catalog
var productCatalog = new Dictionary<int, (string Name, decimal AuthoritativePrice)>
{
    { 101, ("Enterprise Cloud License", 999.00m) },
    { 102, ("Developer Seat", 49.00m) }
};

var orders = new List<OrderRecord>();

// Concurrent dictionary to atomically enforce single coupon redemption per user
var userRedeemedCoupons = new ConcurrentDictionary<string, byte>();

// =========================================================================================
// REMEDIATION #1: Insecure Design & Price Trust FIXED
// Client ONLY sends ProductId and Quantity. The server resolves the price authoritatively
// from the database catalog and strictly validates numeric boundaries.
// =========================================================================================
app.MapPost("/api/checkout/purchase", (SecurePurchaseRequest request) =>
{
    // Validate quantity bounds
    if (request.Quantity <= 0 || request.Quantity > 100)
    {
        return Results.BadRequest(new { Message = "Quantity must be between 1 and 100." });
    }

    // Lookup authoritative price from database
    if (!productCatalog.TryGetValue(request.ProductId, out var product))
    {
        return Results.BadRequest(new { Message = "Product not found." });
    }

    // Authoritative calculation on the backend
    decimal total = product.AuthoritativePrice * request.Quantity;

    var order = new OrderRecord(Guid.NewGuid(), request.ProductId, request.Quantity, total, DateTime.UtcNow);
    lock (orders)
    {
        orders.Add(order);
    }

    return Results.Ok(new
    {
        Message = "Order processed securely with authoritative server pricing.",
        Order = order
    });
});

// =========================================================================================
// REMEDIATION #2: Rate Limiting & Replay Attack Defense FIXED
// Protected by rate limiting and atomic single-use state per user account.
// =========================================================================================
app.MapPost("/api/promotions/apply-coupon", (SecureCouponRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.CouponCode))
    {
        return Results.BadRequest(new { Message = "Invalid request payload." });
    }

    if (request.CouponCode != "SAVE50")
    {
        return Results.BadRequest(new { Message = "Invalid or expired coupon code." });
    }

    string claimKey = $"{request.UserId}:{request.CouponCode}";

    // Atomic check-and-add to prevent race conditions / duplicate redemptions
    if (!userRedeemedCoupons.TryAdd(claimKey, 1))
    {
        return Results.Conflict(new { Message = "Coupon has already been redeemed by this account." });
    }

    return Results.Ok(new { DiscountPercentage = 50, Message = "50% discount applied successfully." });
}).RequireRateLimiting("promotionPolicy");

app.Run();

public record SecurePurchaseRequest(int ProductId, int Quantity);
public record SecureCouponRequest(string CouponCode, string UserId);
public record OrderRecord(Guid OrderId, int ProductId, int Quantity, decimal TotalAmount, DateTime CreatedAt);
