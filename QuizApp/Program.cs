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

// REST API
app.MapGet("/api/quiz/questions", () => Results.Ok(QuizBank.Questions));
app.MapGet("/api/quiz/health", () => Results.Ok(new { Status = "Healthy", App = "KahootCloneSecurityQuiz", Standard = "OWASP Top 10:2025" }));

// SignalR Hub
app.MapHub<QuizHub>("/quizhub");

app.Run();

public static class QuizBank
{
    public static readonly List<QuizQuestion> Questions = new()
    {
        new(
            1,
            "A01:2025 – Broken Access Control & SSRF",
            "In OWASP 2025, where is Server-Side Request Forgery (SSRF) categorized alongside BOLA/IDOR?",
            new[] {
                "Under Injection",
                "Consolidated into Broken Access Control",
                "Under Insecure Design",
                "Under Cryptographic Failures"
            },
            1, // B is correct
            "In OWASP Top 10:2025, SSRF is consolidated into A01: Broken Access Control because it is fundamentally a failure of boundary access control."
        ),
        new(
            2,
            "A02:2025 – Security Misconfiguration",
            "Security Misconfiguration rose to #2 in OWASP 2025. What is the most severe cloud/API misconfiguration?",
            new[] {
                "Using dark mode theme",
                "Exposing unhandled stack traces & wildcard CORS with credentials",
                "Writing unit tests in TypeScript",
                "Enabling HTTPS port 443"
            },
            1, // B is correct
            "Exposing internal stack traces leaks system internals, while wildcard CORS with credentials allows attackers to hijack sensitive user sessions."
        ),
        new(
            3,
            "A03:2025 – Software Supply Chain Failures",
            "A03:2025 expands beyond vulnerable libraries. Which scenario represents a Software Supply Chain Failure?",
            new[] {
                "Compromised build pipeline (CI/CD) injecting malicious code into released packages",
                "Slow database query execution",
                "Writing code without comments",
                "Using CSS flexbox instead of grid"
            },
            0, // A is correct
            "A03:2025 encompasses the full supply chain: compromised build systems, typosquatted package registries, unverified dependencies, and CI/CD pipelines."
        ),
        new(
            4,
            "A04:2025 – Cryptographic Failures",
            "A developer stores user passwords using MD5 and encrypts tokens using DES in ECB mode. What is required?",
            new[] {
                "Base64 encoding is sufficient",
                "Upgrade to salted Argon2id/PBKDF2 and authenticated AES-256-GCM",
                "Double-MD5 hashing with math.random",
                "No change if running over TLS"
            },
            1, // B is correct
            "MD5 and DES/ECB are completely broken. Modern systems require salted slow key-derivation (Argon2id/PBKDF2) and authenticated encryption (AES-256-GCM)."
        ),
        new(
            5,
            "A05:2025 – Injection",
            "An API builds queries via string interpolation: \"SELECT * FROM Users WHERE Email = '\" + email + \"'\". What is the definitive fix?",
            new[] {
                "Escape single quotes manually with regex",
                "Strict Parameterized Queries / Prepared Statements",
                "Base64 encode the email",
                "Use Web Application Firewall only"
            },
            1, // B is correct
            "Parameterized queries ensure the database engine treats input strictly as data parameters, making SQL injection impossible."
        ),
        new(
            6,
            "A06:2025 – Insecure Design",
            "A web checkout endpoint permits users to apply the same 20% discount coupon in a loop until total price is $0. What is this?",
            new[] {
                "Cross-Site Scripting (XSS)",
                "Buffer Overflow",
                "Insecure Design (Business Logic Flaw)",
                "Cryptographic Salt Failure"
            },
            2, // C is correct
            "Insecure Design covers business logic flaws and architectural omissions that cannot be fixed by syntax-level patches alone."
        ),
        new(
            7,
            "A07:2025 – Authentication Failures",
            "An authentication endpoint allows 100,000 rapid password guesses without delay or lockout. Which defense is essential?",
            new[] {
                "CORS header configuration",
                "Rate Limiting & Account Lockout Policies",
                "HTML entity encoding",
                "Database indexing"
            },
            1, // B is correct
            "A07:2025 covers authentication weaknesses; rate limiting and progressive lockouts are vital against brute-force and credential stuffing."
        ),
        new(
            8,
            "A08:2025 – Software and Data Integrity Failures",
            "An application deserializes arbitrary object streams from untrusted HTTP headers using BinaryFormatter. What is the risk?",
            new[] {
                "High network latency",
                "Remote Code Execution (RCE) via gadget chains",
                "CSS styling layout shifts",
                "DNS lookup timeout"
            },
            1, // B is correct
            "Insecure deserialization allows arbitrary code execution. Modern applications must use type-safe JSON serializers without polymorphic object types."
        ),
        new(
            9,
            "A09:2025 – Security Logging & Alerting Failures",
            "OWASP 2025 elevated A09 to include active Alerting. What is a key failure in this category?",
            new[] {
                "Swallowing failed authentication exceptions silently without triggering SIEM alerts",
                "Logging to stdout in local development",
                "Using log level Info instead of Debug",
                "Using Winston or Serilog"
            },
            0, // A is correct
            "A09:2025 emphasizes that logs alone are insufficient—failures must actively alert SIEM and incident response systems to prevent protracted attacker dwell time."
        ),
        new(
            10,
            "A10:2025 – Mishandling of Exceptional Conditions",
            "Brand new in 2025: When an authorization microservice throws an unexpected timeout exception, the code catches it and grants admin access. What is this flaw?",
            new[] {
                "Failing Open (Insecure Default on Exception)",
                "Failing Closed",
                "Cryptographic Salting",
                "Thread pooling error"
            },
            0, // A is correct
            "Failing Open is a critical A10:2025 flaw: when errors occur, systems must fail securely ('Fail-Closed'), denying access rather than defaulting to permissive states."
        )
    };
}

public class QuizHub : Hub
{
    private static readonly ConcurrentDictionary<string, QuizRoom> Rooms = new();
    private static readonly List<QuizQuestion> QuestionBank = QuizBank.Questions;

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
