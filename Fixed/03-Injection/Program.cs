using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Setup in-memory SQLite database
var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();

using (var initCmd = connection.CreateCommand())
{
    initCmd.CommandText = @"
        CREATE TABLE Users (Id INTEGER PRIMARY KEY, Username TEXT, Role TEXT, SecretApiKey TEXT);
        INSERT INTO Users VALUES (1, 'alice', 'User', 'API-KEY-ALICE-123');
        INSERT INTO Users VALUES (2, 'bob', 'User', 'API-KEY-BOB-456');
        INSERT INTO Users VALUES (3, 'admin', 'Administrator', 'SUPER-SECRET-ADMIN-KEY-999');

        CREATE TABLE Products (Id INTEGER PRIMARY KEY, Name TEXT, Price DECIMAL);
        INSERT INTO Products VALUES (1, 'Laptop', 1200.00);
        INSERT INTO Products VALUES (2, 'Wireless Mouse', 25.50);
        INSERT INTO Products VALUES (3, 'Mechanical Keyboard', 85.00);
    ";
    initCmd.ExecuteNonQuery();
}

// =========================================================================================
// REMEDIATION #1: SQL Injection FIXED
// Uses parameterized SQL command with @query parameter. User input is treated strictly as data.
// =========================================================================================
app.MapGet("/api/products/search", (string query) =>
{
    var results = new List<object>();

    using var cmd = connection.CreateCommand();
    // Parameterized query: immune to SQL injection
    cmd.CommandText = "SELECT Id, Name, Price FROM Products WHERE Name LIKE @query";
    cmd.Parameters.AddWithValue("@query", $"%{query}%");

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        results.Add(new
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Price = reader.GetDecimal(2)
        });
    }

    return Results.Ok(results);
});

// =========================================================================================
// REMEDIATION #2: OS Command Injection FIXED
// Eliminates process / shell execution entirely. Uses native .NET Ping API with hostname validation.
// =========================================================================================
app.MapGet("/api/tools/ping", async (string host) =>
{
    // Strict input validation: alphanumeric and dots only (valid hostname or IPv4 format)
    if (string.IsNullOrWhiteSpace(host) || !Regex.IsMatch(host, @"^[a-zA-Z0-9\.\-]+$") || host.Length > 255)
    {
        return Results.BadRequest(new { Message = "Invalid host format." });
    }

    try
    {
        using var pinger = new Ping();
        var reply = await pinger.SendPingAsync(host, 2000);

        return Results.Ok(new
        {
            Host = host,
            Status = reply.Status.ToString(),
            RoundTripTimeMs = reply.RoundtripTime,
            Address = reply.Address?.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ping error: {ex.Message}");
    }
});

app.Run();
