using System.Collections.Concurrent;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// SignalR with IIS-resilient keepalive and client timeout
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

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

// Forwarded headers support for reverse proxies and IIS
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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
        // ----------------------------------------------------------------------------------------------------
        // Q1: Object-Level Authorization (IDOR / BOLA) -> Correct: C (Yellow, 2)
        // ----------------------------------------------------------------------------------------------------
        new(
            1,
            "API Resource Access",
            "An API endpoint '/api/documents/{docId}' retrieves confidential PDF invoices. Which defensive design pattern prevents Alice from accessing Bob's invoice simply by guessing the ID?",
            new[] {
                "Encrypt document IDs using AES-256 before returning them to client browsers",
                "Obfuscate integer IDs with non-sequential UUIDv4 values across all endpoints",
                "Authorize on the server that the caller's TenantId and UserId own the record",
                "Sign outgoing PDF download URLs with short-lived HMAC query signatures"
            },
            2, // C is correct
            "<strong>Correct Answer: C.</strong> Verifying that the authenticated caller owns the requested resource prevents unauthorized data access.<br><br>🛡️ <strong>Belongs to OWASP A01:2025 – Broken Access Control:</strong> Access control must be enforced server-side. Missing object-level authorization (IDOR / BOLA) allows authenticated users to act outside their intended permissions by tampering with object parameters."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q2: Server-Side Request Forgery / Egress Access Control -> Correct: A (Red, 0)
        // ----------------------------------------------------------------------------------------------------
        new(
            2,
            "Remote Webhooks & Egress",
            "A web application allows users to supply a custom 'webhookUrl' to receive event notifications. If the server fetches this URL directly without validation, what is the most critical risk?",
            new[] {
                "The server is coerced into accessing internal networks and cloud metadata",
                "The outbound HTTP connection pool exhausts ephemeral TCP sockets during spikes",
                "Malicious webhook receivers return oversized payloads that exhaust server RAM",
                "DNS rebinding forces the gateway to downgrade transport encryption to TLS 1.0"
            },
            0, // A is correct
            "<strong>Correct Answer: A.</strong> Unrestricted URL fetching lets attackers coerce the backend into acting as a proxy into internal network zones.<br><br>🛡️ <strong>Belongs to OWASP A01:2025 – Broken Access Control (SSRF):</strong> In the official OWASP 2025 standard, Server-Side Request Forgery was consolidated directly into A01 because it is fundamentally a failure to enforce access control boundaries on outbound network requests."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q3: Diagnostic Information Leaks & Error Handling -> Correct: D (Green, 3)
        // ----------------------------------------------------------------------------------------------------
        new(
            3,
            "Application Diagnostics",
            "When an unexpected database connection error crashes an API endpoint in production, what should the HTTP response returned to the caller contain?",
            new[] {
                "The raw SQL exception message and database server hostname for client diagnostics",
                "An HTTP 500 status code with an empty response body and no explanatory headers",
                "The inner exception stack trace filtered to omit line numbers and file paths",
                "A generic error message with a unique correlation ID for server-side lookup"
            },
            3, // D is correct
            "<strong>Correct Answer: D.</strong> Production responses must return sanitized, standardized error responses (e.g. RFC 7807 ProblemDetails) with unique correlation IDs.<br><br>🛡️ <strong>Belongs to OWASP A02:2025 – Security Misconfiguration:</strong> Security Misconfiguration rose to #2 in 2025. Leaking internal stack traces, framework versions, or database schemas provides attackers with actionable reconnaissance to exploit the system."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q4: Cross-Origin Resource Sharing (CORS) & Trust Boundaries -> Correct: B (Blue, 1)
        // ----------------------------------------------------------------------------------------------------
        new(
            4,
            "Cross-Origin Policies",
            "An API team configures their service with 'Access-Control-Allow-Origin: *' while also setting 'Access-Control-Allow-Credentials: true'. Why is this configuration hazardous?",
            new[] {
                "Intermediate web proxies cache sensitive authorization headers across public CDNs",
                "Malicious sites can read sensitive credentialed data from authenticated users",
                "Client browsers automatically disable cross-site request forgery protections",
                "The web server automatically downgrades HTTPS session cookies to plaintext HTTP"
            },
            1, // B is correct
            "<strong>Correct Answer: B.</strong> Wildcard origins combined with credential sharing completely undermine the browser's Same-Origin Policy.<br><br>🛡️ <strong>Belongs to OWASP A02:2025 – Security Misconfiguration:</strong> Misconfigured CORS headers, default admin credentials, and absent defense-in-depth headers represent server and cloud misconfigurations that expose internal systems to foreign web contexts."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q5: Dependency Provenance & Build Pipeline Integrity -> Correct: C (Yellow, 2)
        // ----------------------------------------------------------------------------------------------------
        new(
            5,
            "Build & Release Pipelines",
            "A popular open-source package your application depends on releases an update after an external contributor's account was compromised. How can your CI/CD pipeline protect the build from executing compromised updates?",
            new[] {
                "Run build workers in isolated Docker containers with non-root user permissions",
                "Enable automatic nightly dependency updates to immediately pull latest patches",
                "Enforce package lockfiles with cryptographic checksums and build audit gates",
                "Compile dependencies from public source repositories during every build step"
            },
            2, // C is correct
            "<strong>Correct Answer: C.</strong> Lockfiles, cryptographic hash verification, and automated build audits prevent malicious package tampering.<br><br>🛡️ <strong>Belongs to OWASP A03:2025 – Software Supply Chain Failures:</strong> In 2025, this category expanded significantly beyond outdated components to target the entire software supply chain, including compromised build pipelines, package registries, and unpinned dependencies."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q6: Package Registry Typosquatting -> Correct: D (Green, 3)
        // ----------------------------------------------------------------------------------------------------
        new(
            6,
            "Package Management",
            "An engineer accidentally types 'Newt0nsoft.Json' instead of 'Newtonsoft.Json' into the project file. If an adversary registered that misspelling on the public registry, what threat occurs?",
            new[] {
                "The project build will fail with an unresolvable metadata reference exception",
                "The package manager falls back to unencrypted HTTP connections to fetch files",
                "A namespace collision prevents developer IDEs from loading IntelliSense models",
                "Malicious install hooks and backdoor binaries execute during the build process"
            },
            3, // D is correct
            "<strong>Correct Answer: D.</strong> Typosquatting injects malicious code directly into developer machines and production environments via deceptive package names.<br><br>🛡️ <strong>Belongs to OWASP A03:2025 – Software Supply Chain Failures:</strong> Supply chain attacks frequently target developer trust by squatting on common package name variations to distribute malware through trusted build tools."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q7: Password Hashing vs. High-Speed Cryptographic Digests -> Correct: A (Red, 0)
        // ----------------------------------------------------------------------------------------------------
        new(
            7,
            "Credential Storage",
            "Why is storing user passwords using single-iteration MD5 or SHA-256 considered fundamentally insecure for modern applications?",
            new[] {
                "Hardware GPUs compute billions of hashes per second to crack passwords",
                "MD5 and SHA hashes leak plaintext characters through length extension flaws",
                "Cryptographic hash collisions allow two distinct users to share one identity",
                "Modern database storage engines require salted passwords to build b-tree indexes"
            },
            0, // A is correct
            "<strong>Correct Answer: A.</strong> Fast hashing algorithms allow attackers who steal a database dump to crack passwords in seconds.<br><br>🛡️ <strong>Belongs to OWASP A04:2025 – Cryptographic Failures:</strong> Cryptographic Failures (ranked #4 in 2025) covers the use of broken algorithms and inadequate key derivation. Passwords must be protected using slow, salted algorithms such as PBKDF2 or Argon2id."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q8: Cipher Modes & Authenticated Encryption -> Correct: B (Blue, 1)
        // ----------------------------------------------------------------------------------------------------
        new(
            8,
            "Data Encryption",
            "When encrypting customer credit card data at rest using AES, why is Electronic Codebook (ECB) mode strongly prohibited?",
            new[] {
                "It enforces a maximum key length of 128 bits regardless of cipher strength",
                "Identical plaintext blocks produce identical ciphertext blocks, leaking patterns",
                "Decryption requires transmitting the initialization vector in plaintext over TLS",
                "It generates ciphertext that is vulnerable to bit-flipping integrity attacks"
            },
            1, // B is correct
            "<strong>Correct Answer: B.</strong> ECB mode lacks an initialization vector (IV) and authentication, preserving visible patterns in encrypted data.<br><br>🛡️ <strong>Belongs to OWASP A04:2025 – Cryptographic Failures:</strong> Data protection requires authenticated symmetric encryption modes like AES-256-GCM, which guarantee confidentiality, uniqueness, and integrity."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q9: Interpreter Boundary & SQL Query Construction -> Correct: D (Green, 3)
        // ----------------------------------------------------------------------------------------------------
        new(
            9,
            "Database Query Construction",
            "A search endpoint builds its database query using string concatenation: 'SELECT * FROM Users WHERE Email = '' + input + '''. What is the definitive fix?",
            new[] {
                "Strip quotes and semicolons with an aggressive input validation regex",
                "URL-encode and HTML-escape the input string before passing to the query",
                "Deploy a web application firewall rule to inspect incoming SQL tokens",
                "Use parameterized queries or prepared statements via your data framework"
            },
            3, // D is correct
            "<strong>Correct Answer: D.</strong> Parameterized queries treat user input strictly as literal data, preventing interpreters from parsing it as executable instructions.<br><br>🛡️ <strong>Belongs to OWASP A05:2025 – Injection:</strong> Injection (ranked #5 in 2025) occurs whenever untrusted data is concatenated directly into interpreters (SQL, LDAP, NoSQL, OS shell)."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q10: Command Injection & Native APIs -> Correct: C (Yellow, 2)
        // ----------------------------------------------------------------------------------------------------
        new(
            10,
            "System Process Execution",
            "A server diagnostic utility allows administrators to ping a host by running 'Process.Start(\"bash\", \"-c ping -c 1 \" + host)'. How should this be designed securely?",
            new[] {
                "Sanitize the host parameter by removing whitespace, ampersands, and pipes",
                "Execute the shell command inside a chroot jail with restricted privileges",
                "Use managed native network APIs and validate the host is a valid IP address",
                "Wrap the command argument in double quotation marks before shell invocation"
            },
            2, // C is correct
            "<strong>Correct Answer: C.</strong> Passing user input to an operating system shell allows command chaining (e.g., '8.8.8.8 && rm -rf /'). Using managed native APIs eliminates the shell entirely.<br><br>🛡️ <strong>Belongs to OWASP A05:2025 – Injection (OS Command Injection):</strong> Safe APIs and strict parameterization prevent user input from hijacking host process execution."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q11: Business Logic & Quantity Invariants -> Correct: A (Red, 0)
        // ----------------------------------------------------------------------------------------------------
        new(
            11,
            "E-Commerce Architecture",
            "A shopping cart API endpoint accepts 'quantity' from the client. An attacker submits 'quantity: -5', causing the subtotal to become negative and issuing a refund to their card. What type of vulnerability is this?",
            new[] {
                "Insecure Design: Lack of business logic and domain invariant validation",
                "Input Sanitization Failure: Missing character encoding for numeric values",
                "Parameter Tampering: Client-side cross-site request forgery manipulation",
                "Race Condition: Concurrent state mutation during payment processing pipeline"
            },
            0, // A is correct
            "<strong>Correct Answer: A.</strong> The code executed without syntax errors, but the architecture lacked business logic invariant validation.<br><br>🛡️ <strong>Belongs to OWASP A06:2025 – Insecure Design:</strong> Insecure Design (ranked #6 in 2025) represents architectural flaws and business logic oversights that cannot be resolved by standard syntax sanitization without threat modeling and boundary validation."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q12: PRNG Predictability & Token Generation -> Correct: D (Green, 3)
        // ----------------------------------------------------------------------------------------------------
        new(
            12,
            "Account Recovery Workflows",
            "A password reset system generates a 6-digit confirmation token using 'new Random().Next(100000, 999999)'. Why is this security design flawed?",
            new[] {
                "The default Random class produces tokens that expire in memory after 60 seconds",
                "Integer truncation in 32-bit runtimes creates modulo bias on 6-digit ranges",
                "System entropy starvation causes token generation to block the thread pool",
                "Clock-seeded PRNG algorithms are mathematically predictable by adversaries"
            },
            3, // D is correct
            "<strong>Correct Answer: D.</strong> Standard PRNGs are deterministic and predictable. Security tokens must be generated using cryptographically secure random number generators.<br><br>🛡️ <strong>Belongs to OWASP A06:2025 – Insecure Design:</strong> Architectural design must specify cryptographically secure primitives (e.g., RandomNumberGenerator) for sensitive operations like password reset and MFA tokens."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q13: Credential Stuffing & Rate Limiting -> Correct: B (Blue, 1)
        // ----------------------------------------------------------------------------------------------------
        new(
            13,
            "User Login & Session",
            "An authentication endpoint accepts 50,000 rapid password attempts per second from the same client IP without restriction. Which defensive control is essential to safeguard user accounts?",
            new[] {
                "Enforce periodic 30-day password expiration policies across all accounts",
                "Apply intelligent rate limiting, progressive delays, and account lockouts",
                "Require users to solve CAPTCHAs during initial registration procedures",
                "Increase the minimum password length requirement from 8 to 16 characters"
            },
            1, // B is correct
            "<strong>Correct Answer: B.</strong> Rate limiting and progressive lockouts mitigate automated bot attacks and credential stuffing.<br><br>🛡️ <strong>Belongs to OWASP A07:2025 – Authentication Failures:</strong> Authentication Failures (formerly Identification and Authentication Failures) covers weaknesses that allow attackers to compromise passwords, keys, or session tokens through brute force."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q14: Token Signature Verification vs. Base64 Decoding -> Correct: C (Yellow, 2)
        // ----------------------------------------------------------------------------------------------------
        new(
            14,
            "Identity & Token Validation",
            "A microservice inspects an incoming JWT bearer token by calling 'jwt.decode(token)' to inspect user roles without calling 'jwt.verify(token)'. What can an attacker do?",
            new[] {
                "Replay previously expired session tokens past their configured lifetime",
                "Trigger a deserialization denial of service inside the JSON parser engine",
                "Forge arbitrary claims and elevate privileges because signatures are ignored",
                "Extract the server's private HMAC signing key from the JWT header metadata"
            },
            2, // C is correct
            "<strong>Correct Answer: C.</strong> Decoding a JWT merely unpacks the Base64 JSON strings. Without verifying the cryptographic HMAC/RSA signature, any client can tamper with claims.<br><br>🛡️ <strong>Belongs to OWASP A07:2025 – Authentication Failures:</strong> Accepting untrusted identity claims without signature verification bypasses authentication entirely."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q15: Insecure Object Deserialization -> Correct: A (Red, 0)
        // ----------------------------------------------------------------------------------------------------
        new(
            15,
            "Payload Parsing & State",
            "A service accepts serialized byte streams from external clients and reconstructs them using 'BinaryFormatter.Deserialize()'. Why is this considered extremely hazardous?",
            new[] {
                "Serialized gadget chains execute arbitrary code during object instantiation",
                "Malformed byte streams trigger unhandled stack overflow crashes in runtimes",
                "Binary serialization leaks server memory addresses through object pointers",
                "Unencrypted streams expose sensitive internal class metadata on wire networks"
            },
            0, // A is correct
            "<strong>Correct Answer: A.</strong> Insecure deserialization of arbitrary object types allows attackers to trigger constructors and getters that execute malicious commands.<br><br>🛡️ <strong>Belongs to OWASP A08:2025 – Software and Data Integrity Failures:</strong> Integrity failures occur when code and data structures are deserialized or evaluated without validating their authenticity and structure."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q16: Webhook Origin Verification & HMAC Signatures -> Correct: B (Blue, 1)
        // ----------------------------------------------------------------------------------------------------
        new(
            16,
            "Third-Party Integrations",
            "A web application receives HTTP POST notifications from an external payment gateway indicating order completion. How should the application confirm the payload was not forged or altered by an attacker?",
            new[] {
                "Verify that the incoming HTTP request originates from an HTTPS connection",
                "Verify the HMAC signature computed over the raw payload with a shared secret",
                "Check that the client IP address matches the public DNS record of the gateway",
                "Require the payment payload to include an unencrypted client session cookie"
            },
            1, // B is correct
            "<strong>Correct Answer: B.</strong> Header fields and IP addresses can be manipulated. Verifying an HMAC signature over the raw request body confirms both origin authenticity and payload integrity.<br><br>🛡️ <strong>Belongs to OWASP A08:2025 – Software and Data Integrity Failures:</strong> Processing external payloads without cryptographic integrity verification allows attackers to spoof critical transactions."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q17: Telemetry Sanitization & Secret Exposure -> Correct: D (Green, 3)
        // ----------------------------------------------------------------------------------------------------
        new(
            17,
            "Application Telemetry",
            "A developer adds the log entry: 'logger.LogInformation(\"Login request received: {Payload}\", JsonSerializer.Serialize(loginModel));'. What security issue does this create?",
            new[] {
                "Log serialization overhead significantly degrades HTTP thread performance",
                "Unescaped JSON formatting creates log injection vulnerabilities in log viewers",
                "Concurrent logging triggers file locking exceptions on central disk storage",
                "Plaintext passwords and credentials are leaked into log files and SIEM sinks"
            },
            3, // D is correct
            "<strong>Correct Answer: D.</strong> Logging complete request payloads exposes plain passwords, credit card numbers, and API tokens to anyone with log access.<br><br>🛡️ <strong>Belongs to OWASP A09:2025 – Security Logging & Alerting Failures:</strong> Logging failures include leaking sensitive data (PII, secrets) into telemetry, in addition to missing audit trails."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q18: Silent Exception Swallowing vs. SIEM Alerting -> Correct: A (Red, 0)
        // ----------------------------------------------------------------------------------------------------
        new(
            18,
            "Incident Detection",
            "An API detects repeated cryptographic decryption errors and signature verification failures, but the code silently catches them with an empty catch block. Why is this dangerous?",
            new[] {
                "Attacks continue undetected because security teams receive no SIEM alerts",
                "Unhandled memory allocations from swallowed exceptions cause runtime leaks",
                "The database connection pool fails to reclaim orphaned socket descriptors",
                "The operating system revokes TLS certificates after repeated silent errors"
            },
            0, // A is correct
            "<strong>Correct Answer: A.</strong> Silently swallowing security exceptions prevents incident responders from detecting active attacks, giving adversaries extended dwell time.<br><br>🛡️ <strong>Belongs to OWASP A09:2025 – Security Logging & Alerting Failures:</strong> The 2025 standard specifically elevated 'Alerting' in A09, emphasizing that logging alone is useless without automated alerts that notify security teams during critical anomalies."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q19: Failing Open vs. Failing Closed (Brand New in 2025!) -> Correct: C (Yellow, 2)
        // ----------------------------------------------------------------------------------------------------
        new(
            19,
            "Error Boundaries & Fallbacks",
            "A microservice checks user authorization with an external identity provider. If the identity provider times out or throws an unhandled exception, the catch block defaults to 'isAuthorized = true' to avoid disrupting the user. What critical vulnerability is this?",
            new[] {
                "Insecure Error Handling: Leaking authentication provider endpoint details",
                "Denial of Service: Identity provider timeout blocks client HTTP threads",
                "Failing Open: Granting access by default when unexpected errors occur",
                "Missing Circuit Breaker: Repeated timeouts cascade across microservices"
            },
            2, // C is correct
            "<strong>Correct Answer: C.</strong> Defaulting to granting access when an exception occurs allows attackers to induce timeouts and bypass authorization controls completely. Systems must always 'Fail Closed' (deny access).<br><br>🛡️ <strong>Belongs to OWASP A10:2025 – Mishandling of Exceptional Conditions:</strong> Brand new in 2025! A10 addresses failures where unexpected states cause systems to fail open, bypass security boundaries, or corrupt critical state."
        ),

        // ----------------------------------------------------------------------------------------------------
        // Q20: Transactional State Management & Resilience -> Correct: B (Blue, 1)
        // ----------------------------------------------------------------------------------------------------
        new(
            20,
            "State Management & Rollbacks",
            "A banking service deducts $500 from Account A, throws an unhandled socket exception while crediting Account B, and leaves the exception unhandled without a database transaction rollback. What vulnerability has occurred?",
            new[] {
                "Insecure Cryptographic Storage: Financial balances are stored unencrypted",
                "Mishandling Exceptional Conditions: Inconsistent state and money leakage",
                "Time-of-Check to Time-of-Use: Concurrent double-spending race condition",
                "Broken Authentication: Lack of multi-factor approval for fund transfers"
            },
            1, // B is correct
            "<strong>Correct Answer: B.</strong> Without atomic transaction boundaries and deterministic rollback handling, unhandled exceptions leave systems in corrupted or exploitable intermediate states.<br><br>🛡️ <strong>Belongs to OWASP A10:2025 – Mishandling of Exceptional Conditions:</strong> Exceptional conditions must be handled with explicit error boundaries, atomic transaction rollbacks, and fail-closed state management."
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
