using System.Diagnostics;
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
// VULNERABILITY #1: SQL Injection via String Interpolation (OWASP A03:2021)
// User input is concatenated directly into SQL command text, allowing arbitrary query execution
// or UNION-based data exfiltration (e.g. searchTerm = "' UNION SELECT Id, SecretApiKey, 0 FROM Users --")
// =========================================================================================
app.MapGet("/api/products/search", (string query) =>
{
    var results = new List<object>();

    // [VULNERABILITY]: String interpolation inside SQL query command
    var sql = $"SELECT Id, Name, Price FROM Products WHERE Name LIKE '%{query}%'";

    using var cmd = connection.CreateCommand();
    cmd.CommandText = sql;

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
// VULNERABILITY #2: OS Command Injection (OWASP A03:2021)
// User-supplied host or IP is passed directly to bash/sh/cmd shell without sanitization,
// enabling command chaining (e.g. host = "8.8.8.8; cat /etc/passwd" or "8.8.8.8 && whoami").
// =========================================================================================
app.MapGet("/api/tools/ping", (string host) =>
{
    // [VULNERABILITY]: Executing shell with raw user input
    var psi = new ProcessStartInfo
    {
        FileName = "/bin/sh",
        Arguments = $"-c \"ping -c 1 {host}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(psi);
    if (process == null) return Results.Problem("Failed to start process.");

    var output = process.StandardOutput.ReadToEnd();
    var error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return Results.Ok(new { Output = output, Error = error });
});

app.Run();
