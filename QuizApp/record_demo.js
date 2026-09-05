import { chromium } from 'playwright';
import path from 'path';
import fs from 'fs';

(async () => {
  const recordingsDir = path.resolve('recordings');
  if (!fs.existsSync(recordingsDir)) {
    fs.mkdirSync(recordingsDir, { recursive: true });
  }

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    recordVideo: {
      dir: recordingsDir,
      size: { width: 1280, height: 720 }
    },
    viewport: { width: 1280, height: 720 }
  });

  const page = await context.newPage();
  console.log("Navigating to Security Quiz on http://localhost:5055...");
  await page.goto('http://localhost:5055');
  await page.waitForSelector('.lobby-btn', { timeout: 10000 });
  await page.waitForTimeout(1500);

  // 1. Click Start Presentation Mode
  console.log("Starting Quiz...");
  await page.click('.lobby-btn');
  await page.waitForSelector('#screen-question.active');
  await page.waitForTimeout(2000);

  // 2. Answer Question 1 (Correct is Option B / btn-opt-1)
  console.log("Answering Question 1...");
  await page.click('#btn-opt-1');
  await page.waitForTimeout(2500);

  // 3. Click to view Leaderboard
  console.log("Proceeding to Leaderboard...");
  await page.click('#reveal-box .btn-next');
  await page.waitForSelector('#screen-leaderboard.active');
  await page.waitForTimeout(2000);

  // 4. Click Next Question
  console.log("Advancing to Question 2...");
  await page.click('#screen-leaderboard .btn-next');
  await page.waitForSelector('#screen-question.active');
  await page.waitForTimeout(1500);

  // 5. Answer Question 2 (Correct is Option C / btn-opt-2)
  console.log("Answering Question 2...");
  await page.click('#btn-opt-2');
  await page.waitForTimeout(2500);

  await context.close();
  await browser.close();

  const videoFiles = fs.readdirSync(recordingsDir).filter(f => f.endsWith('.webm') && f.startsWith('page@'));
  if (videoFiles.length > 0) {
    const latest = videoFiles.map(f => ({ name: f, time: fs.statSync(path.join(recordingsDir, f)).mtimeMs }))
      .sort((a, b) => b.time - a.time)[0];
    const target = path.join(recordingsDir, 'secure-kahoot-demo.webm');
    fs.copyFileSync(path.join(recordingsDir, latest.name), target);
    console.log(`Security Quiz video saved to: ${target}`);
  }
})();
