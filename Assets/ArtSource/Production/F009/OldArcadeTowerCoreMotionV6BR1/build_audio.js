const fs = require('fs');
const path = require('path');

const sampleRate = 48000;
const duration = 1.5;
const count = Math.round(sampleRate * duration);
const samples = new Float64Array(count);
let randomState = 0x5a17c9d3;

function randomSigned() {
  randomState = (Math.imul(randomState, 1664525) + 1013904223) >>> 0;
  return randomState / 0xffffffff * 2 - 1;
}

function addTone(start, length, frequency, amplitude, decay = 6, phase = 0) {
  const first = Math.max(0, Math.floor(start * sampleRate));
  const last = Math.min(count, Math.ceil((start + length) * sampleRate));
  for (let i = first; i < last; i += 1) {
    const local = i / sampleRate - start;
    const env = Math.exp(-local * decay) * Math.sin(Math.PI * Math.min(1, local / .008));
    samples[i] += Math.sin(Math.PI * 2 * frequency * local + phase) * amplitude * env;
  }
}

function addNoise(start, length, amplitude, decay = 20) {
  const first = Math.max(0, Math.floor(start * sampleRate));
  const last = Math.min(count, Math.ceil((start + length) * sampleRate));
  let previous = 0;
  for (let i = first; i < last; i += 1) {
    const local = i / sampleRate - start;
    const raw = randomSigned();
    const filtered = raw * .72 + previous * .28;
    previous = filtered;
    const env = Math.exp(-local * decay) * Math.sin(Math.PI * Math.min(1, local / .003));
    samples[i] += filtered * amplitude * env;
  }
}

function addRelay(time, weight) {
  addNoise(time, .035, .22 * weight, 44);
  addTone(time, .048, 1280, .13 * weight, 40);
  addTone(time + .004, .065, 315, .12 * weight, 31, .7);
}

function addLampTicks(start, end, spacing, amplitude) {
  for (let t = start; t <= end + .0001; t += spacing) {
    addNoise(t, .012, amplitude, 95);
    addTone(t, .018, 1760, amplitude * .34, 85);
  }
}

for (let i = 0; i < count; i += 1) {
  const time = i / sampleRate;
  const rise = Math.max(0, Math.min(1, (time - .05) / .82));
  const settle = time < 1.08 ? 1 : Math.max(.22, 1 - (time - 1.08) / .42 * .72);
  const brownout = time >= .815 && time <= .89 ? .12 : 1;
  const hum = Math.sin(Math.PI * 2 * 58 * time) * .013 + Math.sin(Math.PI * 2 * 116 * time + .4) * .006;
  samples[i] += hum * rise * settle * brownout;
}

addLampTicks(.08, .31, .046, .052);
addRelay(.335, .72);
addLampTicks(.27, .51, .043, .046);
addRelay(.535, .62);
addLampTicks(.47, .735, .037, .062);
addRelay(.755, 1.0);

addNoise(.818, .050, .10, 18);
addTone(.818, .065, 87, .08, 26);
addRelay(.902, 1.45);
addTone(.906, .24, 54, .42, 14);
addTone(.91, .17, 108, .20, 18, .35);
addTone(.918, .13, 720, .14, 30);
addNoise(.918, .09, .24, 34);
addNoise(.955, .045, .18, 60);
addTone(.978, .11, 1880, .075, 35);

let peak = 0;
for (const sample of samples) peak = Math.max(peak, Math.abs(sample));
const gain = peak > .88 ? .88 / peak : 1;
const data = Buffer.alloc(count * 2);
for (let i = 0; i < count; i += 1) {
  const value = Math.max(-1, Math.min(1, samples[i] * gain));
  data.writeInt16LE(Math.round(value * 32767), i * 2);
}

const header = Buffer.alloc(44);
header.write('RIFF', 0);
header.writeUInt32LE(36 + data.length, 4);
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
header.writeUInt32LE(data.length, 40);

const output = path.join(__dirname, 'f009_v6b_r1_old_arcade_tower_core_temp_audio_20260721.wav');
fs.writeFileSync(output, Buffer.concat([header, data]));
process.stdout.write(`${output}\n`);
