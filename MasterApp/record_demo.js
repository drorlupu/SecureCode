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
      size: { width: 1440, height: 900 }
    },
    viewport: { width: 1440, height: 900 }
  });

  const page = await context.newPage();
  console.log("Navigating to Master Dashboard on http://localhost:5050...");
  await page.goto('http://localhost:5050');
  await page.waitForSelector('.tile', { timeout: 10000 });
  await page.waitForTimeout(1500);

  // 1. Click on Tile 01 to invoke Vulnerable demo in internal iframe
  console.log("Clicking Tile 01 to launch vulnerable iframe...");
  const firstTile = await page.locator('.tile').first();
  await firstTile.click();

  // Wait for modal and iframe to load
  await page.waitForSelector('#modal-backdrop.active');
  const iframeElement = await page.waitForSelector('#demo-iframe');
  await page.waitForTimeout(3500); // Allow backend service to spin up and load

  // Interact inside the iframe: trigger IDOR
  try {
    const frame = await iframeElement.contentFrame();
    if (frame) {
      await frame.waitForSelector('#btn-fetch-bob', { timeout: 5000 });
      await frame.click('#btn-fetch-bob');
      await page.waitForTimeout(2000);
    }
  } catch (e) {
    console.log("Iframe interaction note:", e.message);
  }

  // 2. Switch to Live Fixed Version tab inside the modal
  console.log("Switching to Fixed Version inside modal...");
  await page.click('#tab-fixed');
  await page.waitForTimeout(3500);

  // Interact inside the fixed iframe
  try {
    const frame = await iframeElement.contentFrame();
    if (frame) {
      await frame.waitForSelector('#btn-fetch-bob', { timeout: 5000 });
      await frame.click('#btn-fetch-bob');
      await page.waitForTimeout(2000);
    }
  } catch (e) {
    console.log("Fixed iframe interaction note:", e.message);
  }

  // 3. Switch to Attack Video tab
  console.log("Switching to Attack Video tab...");
  await page.click('#tab-video-vuln');
  await page.waitForTimeout(2500);

  // 4. Close modal and return to dashboard
  console.log("Closing modal...");
  await page.keyboard.press('Escape');
  await page.waitForTimeout(1000);

  // 5. Click the "Fixed Version" button directly on Tile 10 (SSRF)
  console.log("Clicking Fixed Version button on Tile 10 (SSRF)...");
  const ssrfTile = await page.locator('.tile').last();
  await ssrfTile.scrollIntoViewIfNeeded();
  await page.waitForTimeout(1000);
  const fixedBtn = await ssrfTile.locator('.btn-fixed');
  await fixedBtn.click();

  await page.waitForSelector('#modal-backdrop.active');
  await page.waitForTimeout(3500);

  // Interact with SSRF fixed iframe
  try {
    const frame = await iframeElement.contentFrame();
    if (frame) {
      await frame.waitForSelector('#btn-fetch', { timeout: 5000 });
      await frame.click('#btn-fetch');
      await page.waitForTimeout(2500);
    }
  } catch (e) {
    console.log("SSRF iframe interaction note:", e.message);
  }

  await page.keyboard.press('Escape');
  await page.waitForTimeout(1500);

  await context.close();
  await browser.close();

  const videoFiles = fs.readdirSync(recordingsDir).filter(f => f.endsWith('.webm') && f.startsWith('page@'));
  if (videoFiles.length > 0) {
    const latest = videoFiles.map(f => ({ name: f, time: fs.statSync(path.join(recordingsDir, f)).mtimeMs }))
      .sort((a, b) => b.time - a.time)[0];
    const target = path.join(recordingsDir, 'owasp-master-app-demo.webm');
    fs.copyFileSync(path.join(recordingsDir, latest.name), target);
    console.log(`Master App demonstration video saved to: ${target}`);
  }
})();
