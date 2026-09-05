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
    }
  });

  const page = await context.newPage();
  await page.goto('http://localhost:5022');
  await page.waitForTimeout(1000);

  // Demo 1: Trigger secure login logging
  await page.click('#btn-login');
  await page.waitForTimeout(2000);

  // Demo 2: Trigger monitored exception with Incident ID
  await page.click('#btn-transfer');
  await page.waitForTimeout(2500);

  await context.close();
  await browser.close();

  const videoFiles = fs.readdirSync(recordingsDir).filter(f => f.endsWith('.webm') && f.startsWith('page@'));
  if (videoFiles.length > 0) {
    const latest = videoFiles.map(f => ({ name: f, time: fs.statSync(path.join(recordingsDir, f)).mtimeMs }))
      .sort((a, b) => b.time - a.time)[0];
    const target = path.join(recordingsDir, 'owasp-a09-logging-failures-fixed.webm');
    fs.copyFileSync(path.join(recordingsDir, latest.name), target);
    console.log(`Video saved to: ${target}`);
  }
})();
