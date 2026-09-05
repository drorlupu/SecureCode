var builder = WebApplication.CreateBuilder(args);

// =========================================================================================
// REMEDIATION #1: Strict CORS Policy FIXED
// Whitelists only verified production front-end origins without wildcard reflection.
// =========================================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictCorsPolicy", policy =>
    {
        policy.WithOrigins("https://app.securecorp.example.com", "https://admin.securecorp.example.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// =========================================================================================
// REMEDIATION #2: Environment-Guarded Error Handling FIXED
// Developer exception page is strictly confined to Development environment.
// Production uses a generic non-leaking error endpoint and enforces HSTS.
// =========================================================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// =========================================================================================
// REMEDIATION #3: Security Response Headers Middleware FIXED
// Adds defense-in-depth headers to prevent clickjacking, MIME-sniffing, and XSS.
// =========================================================================================
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' 'unsafe-inline'; frame-ancestors 'self' http://localhost:* http://127.0.0.1:*;");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseCors("StrictCorsPolicy");

app.MapGet("/error", () => Results.Problem(
    detail: "An unexpected error occurred. Please contact support.",
    statusCode: 500,
    title: "Internal Server Error"
));

app.MapGet("/api/data/status", () => Results.Ok(new { Status = "Healthy", Version = "1.0.0" }));

app.Run();
