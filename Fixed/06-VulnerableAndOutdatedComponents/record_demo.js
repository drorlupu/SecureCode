import { chromium } from 'playwright';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function run() {
  const recordingsDir = path.join(__dirname, 'recordings');
  if (!fs.existsSync(recordingsDir)) fs.mkdirSync(recordingsDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1280, height: 720 },
    recordVideo: { dir: recordingsDir, size: { width: 1280, height: 720 } }
  });
  const page = await context.newPage();

  console.log('Navigating to http://localhost:5016...');
  await page.goto('http://localhost:5016');
  await page.waitForTimeout(2000);

  console.log('Step 1: Verifying clean dependency security audit...');
  await page.click('#btn-audit');
  await page.waitForTimeout(3500);

  await page.close();
  const video = page.video();
  let videoPath = video ? await video.path() : null;
  await context.close();
  await browser.close();

  if (videoPath && fs.existsSync(videoPath)) {
    const finalDest = path.join(recordingsDir, 'owasp-a06-vulnerable-components-fixed.webm');
    fs.copyFileSync(videoPath, finalDest);
    console.log('SUCCESS: Video saved to:', finalDest);
  }
}

run().catch(console.error);
