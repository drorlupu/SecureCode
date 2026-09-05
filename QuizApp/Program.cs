using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Question Bank for the Workshop
var questions = new List<QuizQuestion>
{
    new(
        1,
        "Broken Object Level Authorization (IDOR)",
        "A backend endpoint '/api/users/{id}' checks if the user is authenticated, then returns profile details for {id}. What is the primary vulnerability?",
        new[] {
            "SQL Injection in URL parameter",
            "Broken Object Level Authorization (BOLA/IDOR)",
            "Cross-Site Request Forgery (CSRF)",
            "Distributed Denial of Service"
        },
        1, // B is correct
        "Authentication proves WHO you are, not WHAT you own. Without checking 'resource.OwnerId == currentUserId', any logged-in user can access other users' data."
    ),
    new(
        2,
        "Trust Boundaries & Input Validation",
        "Your frontend TypeScript code validates user input using Zod before calling the backend C# API. Is the backend safe from malicious payloads?",
        new[] {
            "Yes, because Zod validates input on the client",
            "Yes, provided HTTPS encryption is active",
            "No! The frontend is across an untrusted Trust Boundary",
            "No, unless strict CORS is configured"
        },
        2, // C is correct
        "Attackers can easily bypass frontend code using curl, Postman, or custom scripts. Frontend validation improves UX; backend validation enforces security!"
    ),
    new(
        3,
        "Cross-Site Scripting (XSS)",
        "In a React application, a developer renders: <div dangerouslySetInnerHTML={{ __html: userBio }} />. What security control is missing?",
        new[] {
            "HTTPS transport encryption",
            "DOM Sanitization (e.g. DOMPurify)",
            "Cross-Origin Resource Sharing (CORS)",
            "SQL Query Parameterization"
        },
        1, // B is correct
        "Injecting raw HTML without DOM sanitization opens direct Stored or Reflected XSS. Always sanitize with trusted libraries like DOMPurify or avoid raw HTML injection."
    ),
    new(
        4,
        "Cryptographic Token Verification",
        "A developer uses 'jwt.decode(token)' in a Node.js route to inspect the user's role and grant access. What is the fatal flaw?",
        new[] {
            "jwt.decode() parses payload without verifying signature",
            "jwt.decode() is too slow for production APIs",
            "jwt.decode() only functions in browser environments",
            "jwt.decode() strips out custom role claims"
        },
        0, // A is correct
        "'jwt.decode()' merely decodes base64 without cryptographic signature verification. Anyone can forge a token with role='admin'. Always use 'jwt.verify()'!"
    ),
    new(
        5,
        "Secure Cryptographic Randomness",
        "You need to generate a password reset token in your application. Which approach is cryptographically secure?",
        new[] {
            "Math.random().toString(36)",
            "crypto.randomBytes(32) / RandomNumberGenerator",
            "Date.now().toString()",
            "md5(username + Date.now())"
        },
        1, // B is correct
        "Standard PRNGs like Math.random() and System.Random are deterministic and predictable. Only CSPRNGs (crypto.randomBytes, RandomNumberGenerator) provide true cryptographic entropy."
    ),
    new(
        6,
        "Software Supply Chain Security (SCA / Mend)",
        "Mend (SCA) flags a Critical CVE in a transitive dependency that cannot immediately be bumped via its parent. What is the best engineering response?",
        new[] {
            "Delete package-lock.json and ignore the alert",
            "Disable Mend in the CI/CD pipeline",
            "Use package.json overrides / direct NuGet reference",
            "Ignore the alert as long as the application builds"
        },
        2, // C is correct
        "Package overrides (npm) and direct package references (.NET) pin transitive dependencies to patched versions immediately while keeping automated tests green!"
    ),
    new(
        7,
        "SQL Injection Defense",
        "An API concatenates: \"SELECT * FROM Users WHERE Email = '\" + email + \"'\". An attacker enters ' OR '1'='1. What is the fundamental fix?",
        new[] {
            "Rely solely on Web Application Firewalls (WAF)",
            "Parameterized Queries / Prepared Statements",
            "Base64-encode the email parameter",
            "Enable TLS/SSL encryption"
        },
        1, // B is correct
        "Parameterized queries treat user input strictly as literal data rather than executable SQL syntax, completely neutralizing SQL injection."
    ),
    new(
        8,
        "Server-Side Request Forgery (SSRF)",
        "An endpoint accepts a user-provided image URL to download avatars. An attacker passes 'http://169.254.169.254/latest/meta-data'. What is this attack?",
        new[] {
            "Server-Side Request Forgery (SSRF)",
            "Cross-Site Request Forgery (CSRF)",
            "Reflected Cross-Site Scripting (XSS)",
            "Clickjacking / UI Redressing"
        },
        0, // A is correct
        "SSRF tricks the backend server into sending unauthorized requests to internal resources, loopback addresses (127.0.0.1), or cloud metadata services (169.254.169.254)."
    )
};

// REST API
app.MapGet("/api/quiz/questions", () => Results.Ok(questions));
app.MapGet("/api/quiz/health", () => Results.Ok(new { Status = "Healthy", App = "KahootCloneSecurityQuiz" }));

// SignalR Hub
app.MapHub<QuizHub>("/quizhub");

app.Run();

public class QuizHub : Hub
{
    private static readonly ConcurrentDictionary<string, QuizRoom> Rooms = new();
    private static readonly List<QuizQuestion> QuestionBank = new()
    {
        new(1, "Access Control", "A backend endpoint '/api/users/{id}' checks if the user is authenticated, then returns profile details for {id}. What is the primary vulnerability?",
            new[] { "SQL Injection", "Broken Object Level Authorization (BOLA/IDOR)", "CSRF", "DDoS" }, 1,
            "Authentication proves WHO you are, not WHAT you own. Resource ownership must be checked server-side."),
        new(2, "Trust Boundaries", "Your frontend TypeScript code validates user input using Zod before calling the backend C# API. Is the backend safe from malicious payloads?",
            new[] { "Yes, validated on client", "Yes, if HTTPS is on", "No! Frontend is across an untrusted Trust Boundary", "No, unless strict CORS is configured" }, 2,
            "Attackers easily bypass frontend code using curl or Postman. Backend validation enforces security."),
        new(3, "XSS Defense", "In a React application, a developer renders: <div dangerouslySetInnerHTML={{ __html: userBio }} />. What security control is missing?",
            new[] { "HTTPS encryption", "DOM Sanitization (e.g. DOMPurify)", "CORS headers", "SQL Parameterization" }, 1,
            "Inserting raw HTML without DOM sanitization opens direct XSS. Always sanitize user markup."),
        new(4, "Token Verification", "A developer uses 'jwt.decode(token)' in a Node.js route to inspect the user's role and grant access. What is the fatal flaw?",
            new[] { "jwt.decode() parses payload without verifying signature", "jwt.decode() is too slow", "jwt.decode() only works in browser", "jwt.decode() strips claims" }, 0,
            "'jwt.decode()' decodes base64 without cryptographic signature verification. Always use 'jwt.verify()'."),
        new(5, "Cryptographic Entropy", "You need to generate a password reset token in your application. Which approach is cryptographically secure?",
            new[] { "Math.random().toString(36)", "crypto.randomBytes(32) / RandomNumberGenerator", "Date.now().toString()", "md5(username + Date.now())" }, 1,
            "Pseudo-random generators are deterministic and predictable. Only CSPRNGs provide cryptographic entropy."),
        new(6, "Supply Chain (SCA)", "Mend (SCA) flags a Critical CVE in a transitive dependency that cannot immediately be bumped via its parent. What is the best engineering response?",
            new[] { "Delete lockfile", "Disable Mend in CI", "Use package.json overrides / direct NuGet reference", "Ignore if code compiles" }, 2,
            "Package overrides pin transitive dependencies to patched versions immediately while keeping automated tests green."),
        new(7, "SQL Injection", "An API concatenates: \"SELECT * FROM Users WHERE Email = '\" + email + \"'\". An attacker enters ' OR '1'='1. What is the fundamental fix?",
            new[] { "WAF only", "Parameterized Queries / Prepared Statements", "Base64-encode input", "SSL encryption" }, 1,
            "Parameterized queries treat user input strictly as literal values, never executable syntax."),
        new(8, "SSRF Protection", "An endpoint accepts a user-provided image URL to download avatars. An attacker passes 'http://169.254.169.254/latest/meta-data'. What is this attack?",
            new[] { "Server-Side Request Forgery (SSRF)", "CSRF", "Reflected XSS", "Clickjacking" }, 0,
            "SSRF tricks the backend server into sending requests to internal resources, loopback addresses, or cloud metadata.")
    };

    public Task<bool> VerifyHostPassword(string password)
    {
        var trimmed = (password ?? "").Trim();
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(trimmed));
        var hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return Task.FromResult(hashHex == "6b391bd77b0c6d046b215744bdca8e36");
    }

    public async Task CreateRoom(string pin)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (!Rooms.TryGetValue(cleanPin, out var room))
        {
            room = new QuizRoom(cleanPin, Context.ConnectionId);
            Rooms[cleanPin] = room;
        }
        else
        {
            room.HostConnectionId = Context.ConnectionId;
        }
        room.Players.TryRemove(Context.ConnectionId, out _);
        await Groups.AddToGroupAsync(Context.ConnectionId, cleanPin);
        await Clients.Caller.SendAsync("RoomCreated", cleanPin);
    }

    public async Task JoinRoom(string pin, string nickname, string avatar)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        var cleanNick = (nickname ?? "").Trim();

        if (string.IsNullOrWhiteSpace(cleanNick))
        {
            await Clients.Caller.SendAsync("JoinFailed", "Nickname cannot be blank. Please enter a valid nickname!");
            return;
        }

        if (!Rooms.TryGetValue(cleanPin, out var room))
        {
            if (cleanPin == "849201")
            {
                room = new QuizRoom(cleanPin, "");
                Rooms[cleanPin] = room;
            }
            else
            {
                await Clients.Caller.SendAsync("JoinFailed", "Game PIN not found. Please verify the PIN displayed on the host screen.");
                return;
            }
        }

        // Do not allow two users to register under the same name
            bool nameTaken = room.Players.Values.Any(p =>
                p.ConnectionId != Context.ConnectionId &&
                string.Equals(p.Nickname.Trim(), cleanNick, StringComparison.OrdinalIgnoreCase));

            if (nameTaken)
            {
                await Clients.Caller.SendAsync("JoinFailed", $"The nickname '{cleanNick}' is already in use by another player. Please choose a different nickname!");
                return;
            }

            var player = new Player(Context.ConnectionId, cleanNick, avatar, 0, 0);
            room.Players[Context.ConnectionId] = player;
            await Groups.AddToGroupAsync(Context.ConnectionId, cleanPin);

            // Notify joining player
            await Clients.Caller.SendAsync("JoinedSuccess", new {
                pin = cleanPin,
                nickname = player.Nickname,
                avatar = player.Avatar,
                totalPlayers = room.Players.Count
            });

            // Notify Host and all players of new registration
            var playerList = room.Players.Values.Select(p => new { p.Nickname, p.Avatar }).ToList();
            await Clients.Group(cleanPin).SendAsync("PlayerJoined", new {
                nickname = player.Nickname,
                avatar = player.Avatar,
                count = room.Players.Count,
                players = playerList
            });
    }

    public async Task StartQuestion(string pin, int questionIndex)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (Rooms.TryGetValue(cleanPin, out var room))
        {
            room.HostConnectionId = Context.ConnectionId;
            room.CurrentQuestionIndex = questionIndex;
            room.QuestionActive = true;
            room.QuestionStartTimeUtc = DateTime.UtcNow;
            room.Players.TryRemove(Context.ConnectionId, out _);

            var activePlayers = room.Players.Values.Where(p => p.ConnectionId != room.HostConnectionId).ToList();
            foreach (var p in activePlayers)
            {
                p.HasAnswered = false;
                p.LastAnswer = null;
                p.ResponseTimeMs = 20000;
            }

            var q = QuestionBank[questionIndex];
            await Clients.Group(cleanPin).SendAsync("QuestionStarted", new {
                index = questionIndex,
                total = QuestionBank.Count,
                category = q.Category,
                prompt = q.Prompt,
                options = q.Options,
                totalPlayers = activePlayers.Count
            });
        }
    }

    public async Task SubmitAnswer(string pin, int questionIndex, int answerIndex, int timeRemainingSec)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (Rooms.TryGetValue(cleanPin, out var room))
        {
            // Host does not play
            if (Context.ConnectionId == room.HostConnectionId) return;
            if (!room.Players.TryGetValue(Context.ConnectionId, out var player)) return;

            if (player.HasAnswered) return;
            player.HasAnswered = true;
            player.LastAnswer = answerIndex;

            // Track precise response time in milliseconds
            var elapsedMs = (DateTime.UtcNow - room.QuestionStartTimeUtc).TotalMilliseconds;
            player.ResponseTimeMs = Math.Clamp(elapsedMs, 80.0, 20000.0);

            // Acknowledge answer submission without revealing if it is correct or incorrect
            await Clients.Caller.SendAsync("AnswerLocked", new {
                choiceIndex = answerIndex
            });

            var activePlayers = room.Players.Values.Where(p => p.ConnectionId != room.HostConnectionId).ToList();
            int answeredCount = activePlayers.Count(p => p.HasAnswered);
            await Clients.Group(cleanPin).SendAsync("PlayerAnswered", new {
                nickname = player.Nickname,
                answeredCount,
                totalCount = activePlayers.Count
            });
        }
    }

    public async Task RevealAnswer(string pin, int questionIndex)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (Rooms.TryGetValue(cleanPin, out var room))
        {
            room.QuestionActive = false;
            var q = QuestionBank[questionIndex];

            var activePlayers = room.Players.Values.Where(p => p.ConnectionId != room.HostConnectionId).ToList();
            var distribution = new int[4];
            foreach (var p in activePlayers)
            {
                if (p.LastAnswer.HasValue && p.LastAnswer.Value >= 0 && p.LastAnswer.Value < 4)
                {
                    distribution[p.LastAnswer.Value]++;
                }
            }

            // Notify everyone of the correct answer and overall vote breakdown
            await Clients.Group(cleanPin).SendAsync("AnswerRevealed", new {
                correctIndex = q.CorrectIndex,
                explanation = q.Explanation,
                distribution
            });

            // Identify correct players ordered strictly by response time (fastest first)
            var correctPlayers = activePlayers
                .Where(p => p.LastAnswer.HasValue && p.LastAnswer.Value == q.CorrectIndex)
                .OrderBy(p => p.ResponseTimeMs)
                .ToList();

            // Deliver individual results to each registered player with speed-scaled points
            foreach (var p in activePlayers)
            {
                bool isCorrect = p.LastAnswer.HasValue && p.LastAnswer.Value == q.CorrectIndex;
                int pointsEarned = 0;
                int speedRank = 0;
                int speedRankBonus = 0;
                double responseSec = Math.Round(p.ResponseTimeMs / 1000.0, 2);

                if (isCorrect)
                {
                    // Speed formula: faster responses earn up to 1,000 base points
                    double timeRatio = Math.Clamp(p.ResponseTimeMs / 20000.0, 0.0, 1.0);
                    int basePoints = (int)Math.Round((1.0 - (timeRatio / 2.0)) * 1000.0);

                    // Speed rank bonus awarding extra points to whoever answers fastest
                    speedRank = correctPlayers.IndexOf(p) + 1;
                    if (speedRank == 1) speedRankBonus = 250; // 1st Fastest
                    else if (speedRank == 2) speedRankBonus = 150; // 2nd Fastest
                    else if (speedRank == 3) speedRankBonus = 75; // 3rd Fastest

                    int streakBonus = p.Streak * 50;
                    pointsEarned = basePoints + speedRankBonus + streakBonus;

                    p.Score += pointsEarned;
                    p.Streak++;
                }
                else
                {
                    p.Streak = 0;
                }

                await Clients.Client(p.ConnectionId).SendAsync("AnswerRecorded", new {
                    isCorrect,
                    pointsEarned,
                    speedRank,
                    speedRankBonus,
                    responseTimeSec = responseSec,
                    totalScore = p.Score,
                    streak = p.Streak,
                    hasAnswered = p.HasAnswered,
                    userChoice = p.LastAnswer,
                    correctIndex = q.CorrectIndex
                });
            }
        }
    }

    public async Task ShowLeaderboard(string pin)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (Rooms.TryGetValue(cleanPin, out var room))
        {
            var leaderboard = room.Players.Values
                .Where(p => p.ConnectionId != room.HostConnectionId)
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.ResponseTimeMs)
                .Take(5)
                .Select((p, idx) => new { rank = idx + 1, nickname = p.Nickname, avatar = p.Avatar, score = p.Score, streak = p.Streak })
                .ToList();

            if (leaderboard.Count == 0)
            {
                leaderboard = new[]
                {
                    new { rank = 1, nickname = "Alice.Sec", avatar = "👾", score = 1850, streak = 2 },
                    new { rank = 2, nickname = "Bob.Dev", avatar = "⚡", score = 1420, streak = 1 },
                    new { rank = 3, nickname = "Carol.Lead", avatar = "🛡️", score = 980, streak = 1 },
                    new { rank = 4, nickname = "Dave.QA", avatar = "🚀", score = 500, streak = 0 }
                }.ToList();
            }

            await Clients.Group(cleanPin).SendAsync("LeaderboardUpdated", leaderboard);
        }
    }

    public async Task FinishQuiz(string pin)
    {
        var cleanPin = pin.Replace(" ", "").Trim();
        if (Rooms.TryGetValue(cleanPin, out var room))
        {
            var podium = room.Players.Values
                .Where(p => p.ConnectionId != room.HostConnectionId)
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.ResponseTimeMs)
                .Take(3)
                .Select((p, idx) => new { rank = idx + 1, nickname = p.Nickname, avatar = p.Avatar, score = p.Score })
                .ToList();

            if (podium.Count == 0)
            {
                podium = new[]
                {
                    new { rank = 1, nickname = "Alice.Sec", avatar = "👾", score = 7850 },
                    new { rank = 2, nickname = "Bob.Dev", avatar = "⚡", score = 6420 },
                    new { rank = 3, nickname = "Carol.Lead", avatar = "🛡️", score = 5190 }
                }.ToList();
            }

            await Clients.Group(cleanPin).SendAsync("QuizFinished", podium);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var room in Rooms.Values)
        {
            if (room.Players.TryRemove(Context.ConnectionId, out var player))
            {
                await Clients.Group(room.Pin).SendAsync("PlayerLeft", new {
                    nickname = player.Nickname,
                    count = room.Players.Count
                });
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}

public class QuizRoom
{
    public string Pin { get; }
    public string HostConnectionId { get; set; }
    public ConcurrentDictionary<string, Player> Players { get; } = new();
    public int CurrentQuestionIndex { get; set; } = -1;
    public bool QuestionActive { get; set; }
    public DateTime QuestionStartTimeUtc { get; set; } = DateTime.UtcNow;

    public QuizRoom(string pin, string hostConnectionId)
    {
        Pin = pin;
        HostConnectionId = hostConnectionId;
    }
}

public class Player
{
    public string ConnectionId { get; set; }
    public string Nickname { get; set; }
    public string Avatar { get; set; }
    public int Score { get; set; }
    public int Streak { get; set; }
    public int? LastAnswer { get; set; }
    public double ResponseTimeMs { get; set; }
    public bool HasAnswered { get; set; }

    public Player(string connectionId, string nickname, string avatar, int score, int streak)
    {
        ConnectionId = connectionId;
        Nickname = nickname;
        Avatar = avatar;
        Score = score;
        Streak = streak;
    }
}

public record QuizQuestion(
    int Id,
    string Category,
    string Prompt,
    string[] Options,
    int CorrectIndex,
    string Explanation
);
