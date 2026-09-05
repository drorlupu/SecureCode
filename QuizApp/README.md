# 🛡️ SecureCode Kahoot! — Corporate Security Quiz Clone

A 100% self-hosted, private Kahoot clone built specifically for enterprise software engineering teams and security workshops.

---

## Real Player Registration & Live Multiplayer

Yes! Real players can connect, register, and play from their own laptops, tablets, or smartphones over the local network or localhost:

1. **Host View (Presenter / Projector)**:
   - Displays the **Game Room PIN** (e.g. `849 201`).
   - Live lobby powered by **ASP.NET Core SignalR**: when real participants register, their chosen avatar and nickname appear instantly on the big screen with an audio chime!
   - Shows real-time answer counter: *"Answers: 12 / 15 submitted"*.
   - Host controls game pacing: starting questions, revealing answers, and advancing through the leaderboard and podium.

2. **Player Controller View (Mobile & Desktop)**:
   - Navigate to `http://<host-ip-or-localhost>:5055?role=player` or click **"📱 Switch to Player View"**.
   - Enter the Game PIN, type your nickname (e.g. `Alex.Dev`), and pick your custom emoji avatar (👾, 🛡️, ⚡, 🚀, 🦊, 🐉, 🤖, 🐱).
   - Once registered, the player is in the live room!
   - When the host starts a question, the player screen transforms into the **4 iconic Kahoot touch buttons**:
     - 🔺 Red Triangle
     - 🔷 Blue Diamond
     - 🟡 Yellow Circle
     - 🟩 Green Square
   - Tapping an answer locks it in and sends it immediately to the server.
   - Shows live individual feedback upon reveal:
     - 🟢 **CORRECT! +850 pts** with animated streak counter (`🔥 Streak: 2`)
     - 🔴 **INCORRECT**
     - Current score and rank!

---

## 8 Realistic Security Engineering Questions

1. **Broken Object Level Authorization (BOLA / IDOR)**: Resource ownership validation (`OwnerId == currentUserId`).
2. **Trust Boundaries & Input Validation**: Why client-side Zod validation alone is insufficient without backend controls.
3. **Cross-Site Scripting (XSS)**: DOM sanitization with DOMPurify vs. `dangerouslySetInnerHTML`.
4. **Cryptographic Token Verification**: `jwt.verify()` signature checking vs. insecure `jwt.decode()`.
5. **Cryptographic Entropy**: Why CSPRNG (`crypto.randomBytes`) is required over `Math.random()`.
6. **Software Supply Chain Security (SCA)**: Resolving critical Mend CVEs via package overrides.
7. **SQL Injection Defense**: Parameterized queries vs. dynamic string concatenation.
8. **Server-Side Request Forgery (SSRF)**: Blocking internal subnets and cloud metadata (`169.254.169.254`).

---

## How to Run & Play

### 1. Start the Quiz Server
```bash
cd QuizApp
dotnet run --urls http://localhost:5055
```

### 2. Host the Game (Projector Screen)
Open **`http://localhost:5055`** in your browser. The Game PIN is displayed on screen.

### 3. Join as Players (Laptops or Phones)
Participants open **`http://localhost:5055?role=player`** (or your machine's LAN IP, e.g. `http://192.168.1.50:5055?role=player`), enter the PIN and their nickname, and tap **"Join Game"**!

---

## Automated Video Demonstrations

- **Multiplayer Live Registration Demo**: [`QuizApp/recordings/secure-kahoot-multiplayer-demo.webm`](./recordings/secure-kahoot-multiplayer-demo.webm)
  *(Demonstrates Tab 1 Host lobby dynamically populating when Tab 2 Player registers, followed by live answering and real-time score updates)*
- **Solo Presentation Mode Demo**: [`QuizApp/recordings/secure-kahoot-demo.webm`](./recordings/secure-kahoot-demo.webm)
