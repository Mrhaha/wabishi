const fs = require('fs');
const os = require('os');
const path = require('path');
const { pathToFileURL } = require('url');
const { spawn } = require('child_process');

const root = __dirname;
const frameDir = path.join(root, '_frames');
const tierDir = path.join(root, '_tier_frames');
const nodeArgs = process.argv.slice(2);
const singleIndex = nodeArgs.indexOf('--single');
const singleTime = singleIndex >= 0 ? Number(nodeArgs[singleIndex + 1]) : null;
const singleOutput = singleIndex >= 0 ? (nodeArgs[singleIndex + 2] || path.join(root, 'preview_test.png')) : null;
const fps = 30;
const duration = 6.2;
const frameCount = Math.round(duration * fps);
const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';

fs.mkdirSync(frameDir, { recursive: true });
fs.mkdirSync(tierDir, { recursive: true });

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

class CdpConnection {
  constructor(url) {
    this.url = url;
    this.nextId = 1;
    this.pending = new Map();
    this.listeners = new Map();
  }
  async open() {
    this.ws = new WebSocket(this.url);
    await new Promise((resolve, reject) => {
      this.ws.addEventListener('open', resolve, { once: true });
      this.ws.addEventListener('error', reject, { once: true });
    });
    this.ws.addEventListener('message', event => {
      const message = JSON.parse(event.data);
      if (message.id && this.pending.has(message.id)) {
        const { resolve, reject } = this.pending.get(message.id);
        this.pending.delete(message.id);
        if (message.error) reject(new Error(`${message.error.message} (${message.error.code})`));
        else resolve(message.result || {});
        return;
      }
      const key = `${message.sessionId || 'browser'}:${message.method}`;
      const callbacks = this.listeners.get(key) || [];
      for (const callback of callbacks) callback(message.params || {});
    });
  }
  send(method, params = {}, sessionId = undefined) {
    const id = this.nextId++;
    const payload = { id, method, params };
    if (sessionId) payload.sessionId = sessionId;
    return new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });
      this.ws.send(JSON.stringify(payload));
    });
  }
  once(method, sessionId = undefined) {
    const key = `${sessionId || 'browser'}:${method}`;
    return new Promise(resolve => {
      const callback = params => {
        const callbacks = this.listeners.get(key) || [];
        this.listeners.set(key, callbacks.filter(item => item !== callback));
        resolve(params);
      };
      const callbacks = this.listeners.get(key) || [];
      callbacks.push(callback);
      this.listeners.set(key, callbacks);
    });
  }
}

async function waitForDevTools(userDataDir, child) {
  const activePort = path.join(userDataDir, 'DevToolsActivePort');
  for (let i = 0; i < 120; i += 1) {
    if (child.exitCode !== null) throw new Error(`Chrome 提前退出：${child.exitCode}`);
    if (fs.existsSync(activePort)) {
      const [port, wsPath] = fs.readFileSync(activePort, 'utf8').trim().split(/\r?\n/);
      if (port && wsPath) return `ws://127.0.0.1:${port}${wsPath}`;
    }
    await delay(50);
  }
  throw new Error('等待 Chrome DevTools 端口超时');
}

async function evaluate(cdp, sessionId, expression, awaitPromise = false) {
  const result = await cdp.send('Runtime.evaluate', {
    expression,
    awaitPromise,
    returnByValue: true,
    userGesture: false
  }, sessionId);
  if (result.exceptionDetails) throw new Error(result.exceptionDetails.text || '页面脚本执行失败');
  return result.result ? result.result.value : undefined;
}

(async () => {
  const userDataDir = fs.mkdtempSync(path.join(os.tmpdir(), 'f009-preview-'));
  const child = spawn(chromePath, [
    '--headless=new',
    '--remote-debugging-port=0',
    `--user-data-dir=${userDataDir}`,
    '--allow-file-access-from-files',
    '--disable-gpu',
    '--hide-scrollbars',
    '--no-first-run',
    '--no-default-browser-check',
    'about:blank'
  ], { stdio: ['ignore', 'ignore', 'ignore'], windowsHide: true });

  let cdp;
  try {
    const browserWs = await waitForDevTools(userDataDir, child);
    cdp = new CdpConnection(browserWs);
    await cdp.open();
    const target = await cdp.send('Target.createTarget', { url: 'about:blank' });
    const attached = await cdp.send('Target.attachToTarget', { targetId: target.targetId, flatten: true });
    const sessionId = attached.sessionId;
    await cdp.send('Page.enable', {}, sessionId);
    await cdp.send('Runtime.enable', {}, sessionId);
    await cdp.send('Emulation.setDeviceMetricsOverride', {
      width: 1280,
      height: 720,
      deviceScaleFactor: 1,
      mobile: false,
      screenWidth: 1280,
      screenHeight: 720
    }, sessionId);

    const loaded = cdp.once('Page.loadEventFired', sessionId);
    await cdp.send('Page.navigate', { url: pathToFileURL(path.join(root, 'preview.html')).href }, sessionId);
    await loaded;
    for (let i = 0; i < 100; i += 1) {
      if (await evaluate(cdp, sessionId, 'window.previewReady === true')) break;
      if (i === 99) throw new Error('预览页面资源加载超时');
      await delay(50);
    }

    async function capture(time, outputPath, format = 'jpeg') {
      await evaluate(cdp, sessionId, `window.renderAt(${Number(time).toFixed(6)})`);
      await evaluate(cdp, sessionId, 'new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))', true);
      await captureCurrent(outputPath, format);
    }

    async function captureCurrent(outputPath, format = 'jpeg') {
      const screenshot = await cdp.send('Page.captureScreenshot', {
        format,
        quality: format === 'jpeg' ? 92 : undefined,
        fromSurface: true,
        captureBeyondViewport: false
      }, sessionId);
      fs.writeFileSync(outputPath, Buffer.from(screenshot.data, 'base64'));
    }

    if (singleTime !== null && Number.isFinite(singleTime)) {
      const outputPath = path.resolve(singleOutput);
      const extension = path.extname(outputPath).toLowerCase();
      const format = extension === '.jpg' || extension === '.jpeg' ? 'jpeg' : 'png';
      await capture(singleTime, outputPath, format);
      process.stdout.write(`SINGLE ${singleTime.toFixed(3)} ${outputPath}\n`);
    } else {
      for (let i = 0; i < frameCount; i += 1) {
        const t = i / fps;
        const framePath = path.join(frameDir, `frame_${String(i).padStart(4, '0')}.jpg`);
        await capture(t, framePath, 'jpeg');
        if (i % fps === 0 || i === frameCount - 1) {
          process.stdout.write(`FRAME ${i + 1}/${frameCount} t=${t.toFixed(2)}\n`);
        }
      }
      const tierScores = [12, 18, 24, 34, 48];
      for (let i = 0; i < tierScores.length; i += 1) {
        await evaluate(cdp, sessionId, `window.renderScoreState(${tierScores[i]})`);
        await evaluate(cdp, sessionId, 'new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))', true);
        const tierPath = path.join(tierDir, `tier_${String(i).padStart(2, '0')}.png`);
        await captureCurrent(tierPath, 'png');
      }
      process.stdout.write('TIER STATES 5/5\n');
    }
    await cdp.send('Browser.close');
  } finally {
    if (child.exitCode === null) child.kill();
    await delay(100);
    if (path.resolve(userDataDir).startsWith(path.resolve(os.tmpdir()))) {
      fs.rmSync(userDataDir, { recursive: true, force: true });
    }
  }
})().catch(error => {
  process.stderr.write(`${error.stack || error}\n`);
  process.exitCode = 1;
});
