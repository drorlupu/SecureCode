var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Server-side authoritative catalog
var productCatalog = new Dictionary<int, (string Name, decimal AuthoritativePrice)>
{
    { 101, ("Enterprise Cloud License", 999.00m) },
    { 102, ("Developer Seat", 49.00m) }
};

var orders = new List<OrderRecord>();
var usedDiscountCodes = new HashSet<string>(); // Mock used coupons

// =========================================================================================
// VULNERABILITY #1: Trusting Client-Controlled Price / Business State (OWASP A04:2021)
// The API accepts 'PricePerUnit' from the client JSON instead of looking up the authoritative
// price on the server. An attacker can modify HTTP payload to purchase for $0.01.
// Also allows negative quantities leading to cash refund theft (Negative Price Exploit).
// =========================================================================================
app.MapPost("/api/checkout/purchase", (PurchaseRequest request) =>
{
    // [INSECURE DESIGN]: Blindly trusting client price!
    decimal itemPrice = request.ClientSpecifiedPrice; // Flaw: Client sets price
    int quantity = request.Quantity; // Flaw: Negative quantity not validated

    decimal total = itemPrice * quantity;

    var order = new OrderRecord(Guid.NewGuid(), request.ProductId, quantity, total, DateTime.UtcNow);
    orders.Add(order);

    return Results.Ok(new
    {
        Message = "Order processed successfully.",
        Order = order
    });
});

// =========================================================================================
// VULNERABILITY #2: Missing Replay & Rate-Limiting Defenses in Discount Logic (OWASP A04:2021)
// Lacks single-use atomicity per account, race-condition protection, and rate limiting.
// =========================================================================================
app.MapPost("/api/promotions/apply-coupon", (CouponRequest request) =>
{
    // [INSECURE DESIGN]: Non-atomic verification without account binding or concurrency locking
    if (request.CouponCode == "SAVE50")
    {
        // Missing checks: Has this user already claimed this?
        // Missing concurrency lock: allows simultaneous requests to redeem multiple times.
        return Results.Ok(new { Discount = 50.00m, Message = "50% discount applied." });
    }

    return Results.BadRequest(new { Message = "Invalid coupon." });
});

app.Run();

public record PurchaseRequest(int ProductId, int Quantity, decimal ClientSpecifiedPrice);
public record CouponRequest(string CouponCode, string UserId);
public record OrderRecord(Guid OrderId, int ProductId, int Quantity, decimal TotalAmount, DateTime CreatedAt);
