import { chromium } from 'playwright';
import path from 'path';
import fs from 'fs';

(async () => {
  const recordingsDir = path.resolve('recordings');
  if (!fs.existsSync(recordingsDir)) {
    fs.mkdirSync(recordingsDir, { recursive: true });
  }

  const browser = await chromium.launch({ headless: true });
  
  // Create shared context with video recording
  const context = await browser.newContext({
    recordVideo: {
      dir: recordingsDir,
      size: { width: 1440, height: 900 }
    },
    viewport: { width: 1440, height: 900 }
  });

  // Tab 1: Host / Presenter View
  const hostPage = await context.newPage();
  console.log("1. Host opening lobby on http://localhost:5055...");
  await hostPage.goto('http://localhost:5055?role=host');
  await hostPage.waitForSelector('#host-pin-display', { timeout: 10000 });
  await hostPage.waitForTimeout(1500);

  // Tab 2: Real Player registering on their device
  const playerPage = await context.newPage();
  console.log("2. Player navigating to join screen...");
  await playerPage.goto('http://localhost:5055?role=player');
  await playerPage.waitForSelector('#p-input-name');
  await playerPage.fill('#p-input-name', 'Dave.Sec');
  await playerPage.waitForTimeout(800);

  // Pick Rocket avatar and register
  console.log("3. Player selecting avatar and joining room...");
  const rocketBtn = await playerPage.locator('.avatar-btn').nth(3);
  await rocketBtn.click();
  await playerPage.click('.btn-join-room');

  // Verify Player Waiting Screen
  await playerPage.waitForSelector('#player-waiting.active');
  console.log("4. Player is registered and in waiting room!");
  await playerPage.waitForTimeout(1500);

  // Switch back to Host view and verify player appeared in live lobby
  await hostPage.bringToFront();
  await hostPage.waitForSelector('.player-tag');
  const playerCount = await hostPage.textContent('#host-player-count');
  console.log(`5. Host lobby dynamically displays connected players: ${playerCount}`);
  await hostPage.waitForTimeout(2000);

  // Host starts the game
  console.log("6. Host starts game...");
  await hostPage.click('#btn-host-start');
  await hostPage.waitForSelector('#host-question.active');
  const initialCounter = await hostPage.textContent('#host-answered-counter');
  console.log(`   Initial counter on Host: "${initialCounter}"`);
  await hostPage.waitForTimeout(1000);

  // Player view: 4 buttons appear and player submits answer
  await playerPage.bringToFront();
  await playerPage.waitForSelector('#player-controller.active');
  await playerPage.waitForTimeout(800);
  console.log("7. Player submitting answer Option B (Blue Diamond)...");
  await playerPage.click('#p-btn-1');
  await playerPage.waitForSelector('#player-submitted-msg');
  await playerPage.waitForTimeout(1000);

  // Host view: check that answered counter updated to 1 / 1
  await hostPage.bringToFront();
  await hostPage.waitForSelector('#host-answered-counter');
  const updatedCounter = await hostPage.textContent('#host-answered-counter');
  console.log(`8. Host answered counter updated: "${updatedCounter}"`);
  await hostPage.waitForTimeout(1000);

  // Host view: reveals answer
  console.log("9. Host clicking Reveal...");
  await hostPage.click('#btn-skip-timer');
  await hostPage.waitForSelector('#host-reveal-card', { state: 'visible' });
  await hostPage.waitForTimeout(2000);

  // Host advances to leaderboard
  console.log("10. Host advancing to leaderboard...");
  await hostPage.click('#host-reveal-card button');
  await hostPage.waitForSelector('#host-leaderboard.active');
  const lbText = await hostPage.textContent('#host-lb-list');
  console.log(`11. Host leaderboard rendered: ${lbText.replace(/\s+/g, ' ').trim()}`);
  await hostPage.waitForTimeout(2000);

  // Player view: verify player leaderboard screen is active and displays standing
  await playerPage.bringToFront();
  await playerPage.waitForSelector('#player-leaderboard.active');
  const playerStanding = await playerPage.textContent('#player-lb-standing');
  const playerScore = await playerPage.textContent('#player-lb-score');
  console.log(`12. Player controller displays: ${playerStanding} | ${playerScore}`);
  await playerPage.waitForTimeout(2000);

  // Back to Host: finish quiz to show podium
  await hostPage.bringToFront();
  console.log("13. Host finishing quiz to show Podium...");
  await hostPage.evaluate(async () => {
    await connection.invoke("FinishQuiz", "849201");
  });
  await hostPage.waitForSelector('#host-podium.active');
  const championName = await hostPage.textContent('#podium-1-name');
  const championScore = await hostPage.textContent('#podium-1-score');
  console.log(`14. Podium Champion: ${championName} with ${championScore}`);
  await hostPage.waitForTimeout(2500);

  await context.close();
  await browser.close();

  // Save the video
  const videoFiles = fs.readdirSync(recordingsDir).filter(f => f.endsWith('.webm') && f.startsWith('page@'));
  if (videoFiles.length > 0) {
    const latest = videoFiles.map(f => ({ name: f, time: fs.statSync(path.join(recordingsDir, f)).mtimeMs }))
      .sort((a, b) => b.time - a.time)[0];
    const target = path.join(recordingsDir, 'secure-kahoot-multiplayer-demo.webm');
    fs.copyFileSync(path.join(recordingsDir, latest.name), target);
    console.log(`Multiplayer demonstration video saved to: ${target}`);
  }
})();
