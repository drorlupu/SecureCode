import { chromium } from 'playwright';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function run() {
  const recordingsDir = path.join(__dirname, 'recordings');
  if (!fs.existsSync(recordingsDir)) {
    fs.mkdirSync(recordingsDir, { recursive: true });
  }

  console.log('Launching Playwright Chromium browser with video recording...');
  const browser = await chromium.launch({
    headless: true
  });

  const context = await browser.newContext({
    viewport: { width: 1280, height: 720 },
    recordVideo: {
      dir: recordingsDir,
      size: { width: 1280, height: 720 }
    }
  });

  const page = await context.newPage();

  console.log('Navigating to http://localhost:5006...');
  await page.goto('http://localhost:5006');
  await page.waitForTimeout(2000);

  // Step 1: Authorized access
  console.log('Step 1: Alice views her own document...');
  await page.click('#btn-fetch-alice');
  await page.waitForTimeout(2500);

  // Step 2: IDOR Defense Demonstration
  console.log("Step 2: Testing IDOR Prevention - Attempting to access Bob's document...");
  await page.click('#bob-doc-id');
  await page.waitForTimeout(1000);
  await page.click('#btn-fetch-bob');
  await page.waitForTimeout(3500);

  // Step 3: Function-Level Access Control Defense
  console.log('Step 3: Testing Admin Reset Policy Enforcement...');
  await page.click('#btn-admin-reset');
  await page.waitForTimeout(3000);

  // Step 4: Mass-Assignment Neutralization
  console.log('Step 4: Testing Role Tampering Neutralization...');
  await page.click('#btn-escalate');
  await page.waitForTimeout(4000);

  // Close context to finalize video
  console.log('Finalizing video recording...');
  await page.close();
  const video = page.video();
  let videoPath = null;
  if (video) {
    videoPath = await video.path();
  }
  await context.close();
  await browser.close();

  if (videoPath && fs.existsSync(videoPath)) {
    const finalDest = path.join(recordingsDir, 'owasp-a01-broken-access-control-fixed.webm');
    fs.copyFileSync(videoPath, finalDest);
    console.log('SUCCESS: Remediated defense video saved to:', finalDest);
  }
}

run().catch(err => {
  console.error('Error running recording:', err);
  process.exit(1);
});
