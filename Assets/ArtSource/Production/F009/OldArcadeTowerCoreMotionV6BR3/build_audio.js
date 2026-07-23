const fs = require('fs');
const path = require('path');

const sampleRate = 48000;
const duration = 4.8;
const length = Math.round(sampleRate * duration);
const mix = new Float64Array(length);
let seed = 0x6b3f009;

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
function addIntakeJaw(time, weight) {
  addRelay(time - .036, .034 + weight * .026, 1710 - weight * 250);
  addTone(time - .020, time + .018, 340 + weight * 55, 255 + weight * 35, .013 + weight * .012, .001, .020);
}

// Cabinet electrical bed: low, steady and intentionally imperfect.
for (let i = 0; i < length; i++) {
  const t = i / sampleRate;
  const drift = 1 + Math.sin(t * Math.PI * 2 * .37) * .018;
  const hum = Math.sin(t * Math.PI * 2 * 58 * drift) * .012 + Math.sin(t * Math.PI * 2 * 116) * .0045;
  mix[i] += hum + rand() * .0017;
}

// Roll motor and face-reel rattle.
addRelay(.075, .14, 1450);
for (let i = Math.floor(.08 * sampleRate); i < Math.floor(1.22 * sampleRate); i++) {
  const t = i / sampleRate;
  const e = envAt(t, .08, .17, 1.08, 1.22);
  const chatter = .54 + .46 * Math.max(0, Math.sin(t * Math.PI * 2 * 20.5));
  const motor = Math.sin(t * Math.PI * 2 * 83) * .025 + Math.sin(t * Math.PI * 2 * 166) * .012;
  mix[i] += (motor + rand() * .026 * chatter) * e;
}
[.89, .95, 1.01, 1.07, 1.13, 1.19].forEach((t, i) => addRelay(t, .105 + i * .008, 1320 - i * 55));
addRelay(1.315, .14, 1040);
addTone(1.31, 1.43, 260, 310, .035, .005, .05);

const events = [
  { travel: 1.51, impact: 1.70, weight: .32, pitch: 405 },
  { travel: 1.78, impact: 1.96, weight: .26, pitch: 430 },
  { travel: 2.03, impact: 2.20, weight: .50, pitch: 470, echo: true },
  { travel: 2.31, impact: 2.49, weight: .43, pitch: 510 },
  { travel: 2.60, impact: 2.78, weight: .48, pitch: 548, target: true },
  { travel: 2.88, impact: 3.08, weight: .70, pitch: 585 },
  { travel: 3.19, impact: 3.43, weight: 1.00, pitch: 625 },
];
events.forEach(e => {
  addFilamentGather(e.travel, e.weight, e.echo);
  addWhoosh(e.travel, e.weight, e.echo);
  addIntakeJaw(e.impact, e.weight);
  addImpact(e.impact, e.weight, e.pitch);
  if (e.echo) {
    addRelay(e.travel - .075, .082, 1540);
    addRelay(e.impact + .046, .070, 1210);
  }
  if (e.target) {
    addRelay(e.impact + .038, .064, 1420);
    addRelay(e.impact + .082, .050, 1180);
    addTone(e.impact + .055, e.impact + .31, 760, 820, .042, .004, .17);
    addTone(e.impact + .10, e.impact + .36, 1140, 1050, .025, .004, .19);
  }
});

// Overload pressure after the largest packet.
addTone(3.39, 3.67, 74, 91, .055, .015, .04);
addTone(3.44, 3.66, 148, 184, .027, .01, .035);
addNoise(3.47, 3.67, .035, .04, .03, .90);
addTone(3.625, 3.785, 1210, 1740, .027, .018, .018);
addRelay(3.718, .10, 1520);

// Hard critical: contact crack, relay slam, sub hit, hot CRT ring.
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

// Stable stage-clear latch.
addRelay(4.085, .14, 910);
addTone(4.10, 4.48, 392, 392, .047, .008, .25);
addTone(4.16, 4.54, 588, 588, .036, .008, .27);

// Whole-cabinet anticipation dip; preserves a low residual hum before the strike.
for (let i = Math.floor(3.61 * sampleRate); i < Math.floor(3.755 * sampleRate); i++) {
  const t = i / sampleRate;
  const down = smooth01((t - 3.61) / .055);
  const up = smooth01((t - 3.705) / .05);
  const gain = .98 - down * .80 + up * .72;
  mix[i] *= clamp(gain, .16, 1);
}

let peak = 0;
let sumSq = 0;
for (const v of mix) { peak = Math.max(peak, Math.abs(v)); sumSq += v * v; }
const gain = peak > 0 ? .86 / peak : 1;
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

const output = path.join(__dirname, 'f009_v6b_r3_detail_polish_critical_temp_audio_20260722.wav');
fs.writeFileSync(output, Buffer.concat([header, pcm]));
const rmsBefore = Math.sqrt(sumSq / length);
console.log(JSON.stringify({ output, sampleRate, duration, peakBefore: peak, rmsBefore, normalizeGain: gain }, null, 2));
