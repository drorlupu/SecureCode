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

  console.log('Navigating to http://localhost:5005...');
  await page.goto('http://localhost:5005');
  await page.waitForTimeout(2000);

  // Step 1: Legitimate Authorized Access (Alice viewing her own document)
  console.log('Step 1: Alice views her own document...');
  await page.click('#btn-fetch-alice');
  await page.waitForTimeout(2500);

  // Step 2: Insecure Direct Object Reference (IDOR / BOLA)
  console.log("Step 2: Demonstrating IDOR - Accessing Bob's confidential medical record...");
  await page.click('#bob-doc-id');
  await page.waitForTimeout(1000);
  await page.click('#btn-fetch-bob');
  await page.waitForTimeout(3500);

  // Step 3: Missing Function-Level Access Control
  console.log('Step 3: Demonstrating Missing Function-Level Access Control (Admin reset)...');
  await page.click('#btn-admin-reset');
  await page.waitForTimeout(3000);

  // Step 4: Mass Assignment / Privilege Escalation
  console.log('Step 4: Demonstrating Role Mass-Assignment / Privilege Escalation...');
  await page.click('#btn-escalate');
  await page.waitForTimeout(4000);

  // Close context to finalize and save the video
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
    const finalDest = path.join(recordingsDir, 'owasp-a01-broken-access-control.webm');
    fs.copyFileSync(videoPath, finalDest);
    console.log('SUCCESS: Video saved to:', finalDest);
  } else {
    console.log('Video recording completed in:', recordingsDir);
  }
}

run().catch(err => {
  console.error('Error running recording:', err);
  process.exit(1);
});
