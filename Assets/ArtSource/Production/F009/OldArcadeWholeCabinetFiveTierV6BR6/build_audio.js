const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
const tierArg = args.indexOf('--tier');
const requestedTier = tierArg >= 0 ? String(args[tierArg + 1] || '') : 'critical';
const tierConfigs = {
  miss: { index: 1, target: 22, deltas: [2, 2, 3, 3, 2, 3, 3], weights: [.18, .16, .24, .24, .18, .26, .32], targetPeak: .50, seed: 0x6b51009 },
  pass: { index: 2, target: 22, deltas: [3, 2, 4, 4, 4, 5, 5], weights: [.24, .18, .33, .33, .35, .42, .45], targetPeak: .58, seed: 0x6b52009 },
  exceed: { index: 3, target: 22, deltas: [4, 3, 5, 5, 6, 7, 8], weights: [.32, .24, .42, .40, .48, .58, .68], targetPeak: .67, seed: 0x6b53009 },
  far: { index: 4, target: 22, deltas: [5, 4, 7, 7, 8, 11, 13], weights: [.40, .32, .55, .55, .62, .78, .90], targetPeak: .76, seed: 0x6b54009 },
  critical: { index: 5, target: 22, deltas: [6, 5, 8, 9, 10, 13, 18], weights: [.32, .26, .50, .43, .48, .70, 1.00], targetPeak: .86, seed: 0x6b55009 }
};
const tierKey = Object.prototype.hasOwnProperty.call(tierConfigs, requestedTier) ? requestedTier : 'critical';
const tier = tierConfigs[tierKey];

const sampleRate = 48000;
const duration = 4.8;
const length = Math.round(sampleRate * duration);
const mix = new Float64Array(length);
let seed = tier.seed;

function clamp(v, lo = 0, hi = 1) { return Math.max(lo, Math.min(hi, v)); }
function smooth01(v) { v = clamp(v); return v * v * (3 - 2 * v); }
function rand() {
  seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
  return seed / 4294967296 * 2 - 1;
}
function envAt(t, start, attack, hold, release) {
  if (t < start || t >= release) return 0;
  if (t < attack) return smooth01((t - start) / Math.max(1e-6, attack - start));
  if (t <= hold) return 1;
  return 1 - smooth01((t - hold) / Math.max(1e-6, release - hold));
}
function addTone(start, end, f0, f1, amp, attack = .006, release = .05, phase = 0) {
  const a = Math.max(0, Math.floor(start * sampleRate));
  const b = Math.min(length, Math.ceil(end * sampleRate));
  let ph = phase;
  for (let i = a; i < b; i++) {
    const t = i / sampleRate;
    const u = clamp((t - start) / Math.max(1e-6, end - start));
    const f = f0 + (f1 - f0) * u;
    ph += Math.PI * 2 * f / sampleRate;
    const e = envAt(t, start, start + attack, end - release, end);
    mix[i] += Math.sin(ph) * amp * e;
  }
}
function addNoise(start, end, amp, attack = .004, release = .04, color = .68) {
  const a = Math.max(0, Math.floor(start * sampleRate));
  const b = Math.min(length, Math.ceil(end * sampleRate));
  let low = 0;
  for (let i = a; i < b; i++) {
    const t = i / sampleRate;
    const e = envAt(t, start, start + attack, end - release, end);
    const white = rand();
    low += (white - low) * (1 - color);
    mix[i] += (white * .55 + low * .45) * amp * e;
  }
}
function addRelay(time, amp = .16, pitch = 1280) {
  addNoise(time, time + .028, amp, .001, .021, .36);
  addTone(time, time + .045, pitch, pitch * .62, amp * .72, .001, .035);
  addTone(time + .018, time + .070, pitch * .43, pitch * .31, amp * .38, .001, .045);
}
function addImpact(time, weight, pitch = 440) {
  const amp = .075 + weight * .105;
  addRelay(time - .008, amp * .72, 980 + pitch * .42);
  addTone(time, time + .19 + weight * .07, pitch, pitch * .78, amp, .002, .13);
  addTone(time, time + .24 + weight * .10, pitch * .5, pitch * .42, amp * .74, .003, .18);
  addNoise(time, time + .08, amp * .39, .001, .06, .53);
}
function addWhoosh(time, weight, echo = false) {
  const dur = .125 + weight * .06;
  addNoise(time - .03, time + dur, (.026 + weight * .034) * (echo ? .74 : 1), .025, .055, .82);
  addTone(time - .012, time + dur, echo ? 720 : 510, echo ? 470 : 760, .018 + weight * .023, .02, .05);
}
function addFilamentGather(time, weight, echo = false) {
  const amp = (.010 + weight * .012) * (echo ? .76 : 1);
  addTone(time - .108, time + .012, echo ? 1180 : 980, echo ? 1510 : 1420, amp, .035, .025);
  addNoise(time - .094, time + .008, amp * .62, .028, .02, .90);
  [time - .073, time - .041, time - .014].forEach((tick, index) => {
    addTone(tick, tick + .014, 1620 + index * 115, 1180 + index * 80, amp * (.52 + index * .13), .001, .010);
  });
}
function addGlassAbsorb(time, weight) {
  const amp = .014 + weight * .018;
  addNoise(time - .018, time + .045, amp, .008, .028, .88);
  addTone(time - .010, time + .082, 690 + weight * 75, 1120 + weight * 120, amp * 1.08, .004, .052);
  addTone(time + .014, time + .142, 1040 + weight * 110, 720 + weight * 80, amp * .46, .004, .085);
}
function addStageClear(strength) {
  addRelay(4.085, .078 + strength * .055, 900 + strength * 24);
  addTone(4.10, 4.48, 392, 392, .026 + strength * .023, .008, .25);
  addTone(4.16, 4.54, 588, 588, .018 + strength * .020, .008, .27);
}
function addCabinetSweep(start, strength, reverse = false, steps = 8) {
  for (let i = 0; i < steps; i++) {
    const order = reverse ? steps - 1 - i : i;
    const time = start + i * (.018 - strength * .0035);
    addRelay(time, .018 + strength * .019, 760 + order * 54 + strength * 180);
  }
  addTone(start - .012, start + .24 + strength * .12, 92 + strength * 16, 128 + strength * 58, .018 + strength * .034, .012, .12);
}
function addCrtBreak(time, strength) {
  addNoise(time - .018, time + .075 + strength * .055, .028 + strength * .082, .004, .045, .25);
  addTone(time - .012, time + .12 + strength * .08, 1760 + strength * 480, 620 + strength * 120, .022 + strength * .047, .002, .08);
  addTone(time, time + .20 + strength * .11, 71, 54, .030 + strength * .072, .002, .16);
}

// Continuous cabinet bed and shared roll sequence stay identical across all five tiers.
for (let i = 0; i < length; i++) {
  const t = i / sampleRate;
  const drift = 1 + Math.sin(t * Math.PI * 2 * .37) * .018;
  const hum = Math.sin(t * Math.PI * 2 * 58 * drift) * .012 + Math.sin(t * Math.PI * 2 * 116) * .0045;
  mix[i] += hum + rand() * .0017;
}

addRelay(.075, .14, 1450);
for (let i = Math.floor(.08 * sampleRate); i < Math.floor(1.22 * sampleRate); i++) {
  const t = i / sampleRate;
  const e = envAt(t, .08, .17, 1.08, 1.22);
  const chatter = .54 + .46 * Math.max(0, Math.sin(t * Math.PI * 2 * 20.5));
  const motor = Math.sin(t * Math.PI * 2 * 83) * .025 + Math.sin(t * Math.PI * 2 * 166) * .012;
  mix[i] += (motor + rand() * .026 * chatter) * e;
}
[.89, .95, 1.01, 1.07, 1.13, 1.19].forEach((time, index) => addRelay(time, .105 + index * .008, 1320 - index * 55));
addRelay(1.315, .14, 1040);
addTone(1.31, 1.43, 260, 310, .035, .005, .05);

const eventTemplate = [
  { travel: 1.51, impact: 1.70, pitch: 405 },
  { travel: 1.78, impact: 1.96, pitch: 430 },
  { travel: 2.03, impact: 2.20, pitch: 470, echo: true },
  { travel: 2.31, impact: 2.49, pitch: 510 },
  { travel: 2.60, impact: 2.78, pitch: 548 },
  { travel: 2.88, impact: 3.08, pitch: 585 },
  { travel: 3.19, impact: 3.43, pitch: 625 }
];
const events = eventTemplate.map((event, index) => ({ ...event, weight: tier.weights[index], delta: tier.deltas[index] }));
let runningScore = 0;
let targetEventIndex = -1;
events.forEach((event, index) => {
  const before = runningScore;
  runningScore += event.delta;
  if (targetEventIndex < 0 && before < tier.target && runningScore >= tier.target) targetEventIndex = index;
});

events.forEach((event, index) => {
  addFilamentGather(event.travel, event.weight, event.echo);
  addWhoosh(event.travel, event.weight, event.echo);
  addGlassAbsorb(event.impact, event.weight);
  addImpact(event.impact, event.weight, event.pitch);
  if (event.echo) {
    addRelay(event.travel - .075, .082, 1540);
    addRelay(event.impact + .046, .070, 1210);
  }
  if (index === targetEventIndex) {
    addRelay(event.impact + .038, .052 + tier.index * .004, 1420);
    addRelay(event.impact + .082, .040 + tier.index * .003, 1180);
    addTone(event.impact + .055, event.impact + .31, 760, 820, .028 + tier.index * .003, .004, .17);
    addTone(event.impact + .10, event.impact + .36, 1140, 1050, .016 + tier.index * .002, .004, .19);
  }
});

if (tierKey === 'miss') {
  addCabinetSweep(3.58, .18, true, 5);
  addRelay(3.57, .052, 690);
  addTone(3.57, 4.18, 178, 76, .054, .018, .12);
  addTone(3.64, 4.12, 356, 128, .022, .025, .12);
  addNoise(3.60, 4.08, .025, .045, .16, .88);
  addRelay(4.10, .070, 610);
  addTone(4.12, 4.52, 196, 174, .022, .012, .27);
} else if (tierKey === 'pass') {
  addCabinetSweep(3.57, .30, false, 6);
  addRelay(3.60, .066, 870);
  addTone(3.58, 3.98, 318, 392, .036, .012, .18);
  addStageClear(.34);
} else if (tierKey === 'exceed') {
  addCabinetSweep(3.54, .52, false, 8);
  addCabinetSweep(3.73, .34, true, 6);
  addCrtBreak(3.79, .16);
  addRelay(3.585, .088, 1080);
  addTone(3.57, 3.88, 382, 474, .050, .006, .16);
  addRelay(3.748, .072, 990);
  addTone(3.74, 4.08, 474, 392, .042, .006, .19);
  addTone(3.56, 4.10, 96, 88, .050, .012, .20);
  addStageClear(.52);
} else if (tierKey === 'far') {
  addCabinetSweep(3.50, .76, false, 10);
  addCabinetSweep(3.72, .48, true, 8);
  addCrtBreak(3.69, .30);
  addCrtBreak(3.83, .56);
  addTone(3.44, 4.22, 74, 96, .118, .025, .22);
  addTone(3.51, 4.18, 148, 192, .060, .018, .20);
  addTone(3.58, 4.18, 660, 910, .056, .010, .23);
  addNoise(3.54, 4.08, .048, .030, .18, .84);
  addRelay(3.63, .118, 1280);
  addRelay(3.82, .084, 1110);
  addStageClear(.72);
} else {
  // R6 critical language: whole-cabinet brownout, two-stage relight, contact slam and hot CRT tear.
  addCabinetSweep(3.54, .88, false, 12);
  addCabinetSweep(3.742, 1.00, false, 14);
  addCrtBreak(3.776, .78);
  addCrtBreak(3.817, 1.00);
  addTone(3.39, 3.67, 74, 91, .055, .015, .04);
  addTone(3.44, 3.66, 148, 184, .027, .01, .035);
  addNoise(3.47, 3.67, .035, .04, .03, .90);
  addTone(3.625, 3.785, 1210, 1740, .027, .018, .018);
  addRelay(3.718, .10, 1520);
  addNoise(3.754, 3.825, .34, .001, .052, .22);
  addRelay(3.766, .31, 1680);
  addTone(3.770, 4.12, 58, 46, .33, .002, .25);
  addTone(3.778, 4.02, 116, 92, .20, .002, .17);
  addTone(3.786, 4.20, 690, 530, .14, .003, .32);
  addTone(3.795, 4.28, 1380, 1080, .075, .003, .39);
  addNoise(3.812, 4.06, .09, .002, .20, .76);
  [3.828, 3.852, 3.879, 3.909, 3.942, 3.978].forEach((time, index) => {
    addRelay(time, .034 - index * .0022, 1280 - index * 70);
  });
  addStageClear(1);

  for (let i = Math.floor(3.61 * sampleRate); i < Math.floor(3.755 * sampleRate); i++) {
    const t = i / sampleRate;
    const down = smooth01((t - 3.61) / .055);
    const up = smooth01((t - 3.705) / .05);
    const dipGain = .98 - down * .80 + up * .72;
    mix[i] *= clamp(dipGain, .16, 1);
  }
}

let peak = 0;
let sumSq = 0;
for (const value of mix) {
  peak = Math.max(peak, Math.abs(value));
  sumSq += value * value;
}
const gain = peak > 0 ? tier.targetPeak / peak : 1;
const pcm = Buffer.alloc(length * 2);
for (let i = 0; i < length; i++) {
  const shaped = Math.tanh(mix[i] * gain * 1.08) / Math.tanh(1.08);
  pcm.writeInt16LE(Math.round(clamp(shaped, -1, 1) * 32767), i * 2);
}

const header = Buffer.alloc(44);
header.write('RIFF', 0);
header.writeUInt32LE(36 + pcm.length, 4);
header.write('WAVE', 8);
header.write('fmt ', 12);
header.writeUInt32LE(16, 16);
header.writeUInt16LE(1, 20);
header.writeUInt16LE(1, 22);
header.writeUInt32LE(sampleRate, 24);
header.writeUInt32LE(sampleRate * 2, 28);
header.writeUInt16LE(2, 32);
header.writeUInt16LE(16, 34);
header.write('data', 36);
header.writeUInt32LE(pcm.length, 40);

const slugs = { miss: '01_miss', pass: '02_pass', exceed: '03_exceed', far: '04_far_exceed', critical: '05_critical' };
const output = path.join(__dirname, `f009_v6b_r6_tier_${slugs[tierKey]}_temp_audio_20260722.wav`);
fs.writeFileSync(output, Buffer.concat([header, pcm]));
const rmsBefore = Math.sqrt(sumSq / length);
console.log(JSON.stringify({ tier: tierKey, output, sampleRate, duration, finalScore: runningScore, targetScore: tier.target, peakBefore: peak, rmsBefore, normalizeGain: gain }, null, 2));
