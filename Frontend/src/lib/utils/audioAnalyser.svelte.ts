let analyser: AnalyserNode;
let dataArray: Uint8Array<ArrayBuffer>;

class AudioAnalyserState {
    glowIntensity = $state(0);
    animationFrame = $state(0);
    glowColor = $state('255, 255, 255');
    glowColorWarm = $state('255, 255, 255');
    glowColorCool = $state('255, 255, 255');
    bass = $state(0);
    mid = $state(0);
    high = $state(0);
    time = $state(0);
    isPaused = $state(false);
    bins = $state<number[]>(new Array(16).fill(0));
}

export const audioAnalyser = new AudioAnalyserState();

export function setupAnalyser(audio: HTMLAudioElement) {
    if(!audioAnalyser.isPaused) {
        const ctx = new AudioContext();
        const source = ctx.createMediaElementSource(audio);
        analyser = ctx.createAnalyser();
        analyser.fftSize = 512;
        dataArray = new Uint8Array(analyser.frequencyBinCount);

        source.connect(analyser);
        analyser.connect(ctx.destination);

        let smoothBass = 0;
        let smoothMid = 0;
        let smoothHigh = 0;
        let smoothBins = new Array(16).fill(0);

        function tick() {
            analyser.getByteFrequencyData(dataArray);

            // Sample 16 evenly spaced bins
            const binSize = Math.floor(dataArray.length / 16);
            for (let i = 0; i < 16; i++) {
                let sum = 0;
                for (let j = 0; j < binSize; j++) {
                    sum += dataArray[i * binSize + j];
                }
                const target = sum / binSize / 255;
                smoothBins[i] += (target - smoothBins[i]) * (target > smoothBins[i] ? 0.4 : 0.08);
            }
            audioAnalyser.bins = [...smoothBins];

            const bassAvg = avg(dataArray, 0, 10);
            const midAvg = avg(dataArray, 10, 80);
            const highAvg = avg(dataArray, 80, 160);

            // Smooth with different attack/release speeds
            smoothBass += (bassAvg - smoothBass) * (bassAvg > smoothBass ? 0.15 : 0.04);
            smoothMid += (midAvg - smoothMid) * (midAvg > smoothMid ? 0.1 : 0.03);
            smoothHigh += (highAvg - smoothHigh) * (highAvg > smoothHigh ? 0.12 : 0.05);

            audioAnalyser.bass = smoothBass / 255;
            audioAnalyser.mid = smoothMid / 255;
            audioAnalyser.high = smoothHigh / 255;
            audioAnalyser.glowIntensity = (smoothBass * 0.6 + smoothMid * 0.3 + smoothHigh * 0.1) / 255;
            audioAnalyser.time = performance.now() / 1000;

            audioAnalyser.animationFrame = requestAnimationFrame(tick);
        }
        tick();
    }

}

function avg(data: Uint8Array, from: number, to: number): number {
    let sum = 0;
    for (let i = from; i < to; i++) sum += data[i];
    return sum / (to - from);
}

export function extractDominantColor(imgSrc: string) {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => {
        const canvas = document.createElement('canvas');
        const size = 32;
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d')!;
        ctx.drawImage(img, 0, 0, size, size);
        const data = ctx.getImageData(0, 0, size, size).data;

        // Sample 4 quadrants
        const colors = [
            sampleRegion(data, size, 0, 0, size / 2, size / 2),           // top-left
            sampleRegion(data, size, size / 2, 0, size, size / 2),        // top-right
            sampleRegion(data, size, 0, size / 2, size / 2, size),        // bottom-left
            sampleRegion(data, size, size / 2, size / 2, size, size),     // bottom-right
        ];

        // Sort by saturation — most vibrant first
        colors.sort((a, b) => saturation(b) - saturation(a));

        // Pick 3 most distinct colors
        const picked = [colors[0]];
        for (const c of colors.slice(1)) {
            if (picked.length >= 3) break;
            if (picked.every(p => colorDistance(p, c) > 50)) {
                picked.push(c);
            }
        }
        // Fill remaining slots if not enough distinct colors
        while (picked.length < 3) {
            picked.push(picked[picked.length - 1]);
        }

        audioAnalyser.glowColor = `${picked[0].r}, ${picked[0].g}, ${picked[0].b}`;
        audioAnalyser.glowColorWarm = `${picked[1].r}, ${picked[1].g}, ${picked[1].b}`;
        audioAnalyser.glowColorCool = `${picked[2].r}, ${picked[2].g}, ${picked[2].b}`;
    };
    img.src = imgSrc;
}

function sampleRegion(
    data: Uint8ClampedArray, width: number,
    x1: number, y1: number, x2: number, y2: number
) {
    let r = 0, g = 0, b = 0, count = 0;
    for (let y = Math.floor(y1); y < Math.floor(y2); y++) {
        for (let x = Math.floor(x1); x < Math.floor(x2); x++) {
            const i = (y * width + x) * 4;
            const brightness = data[i] + data[i + 1] + data[i + 2];
            if (brightness > 60 && brightness < 700) {
                r += data[i];
                g += data[i + 1];
                b += data[i + 2];
                count++;
            }
        }
    }
    if (count === 0) return { r: 128, g: 128, b: 128 };
    return {
        r: Math.round(r / count),
        g: Math.round(g / count),
        b: Math.round(b / count)
    };
}

function saturation(c: { r: number; g: number; b: number }) {
    const max = Math.max(c.r, c.g, c.b);
    const min = Math.min(c.r, c.g, c.b);
    return max === 0 ? 0 : (max - min) / max;
}

function colorDistance(a: { r: number; g: number; b: number }, b: { r: number; g: number; b: number }) {
    return Math.sqrt((a.r - b.r) ** 2 + (a.g - b.g) ** 2 + (a.b - b.b) ** 2);
}
export function blobPath(bins: number[], baseRadius: number, timeOffset: number, amplitude: number): string {
    const points = bins.length;
    const coords: [number, number][] = [];

    for (let i = 0; i < points; i++) {
        const angle = (i / points) * Math.PI * 2;
        const noise = Math.sin(timeOffset + i * 0.8) * 0.05;
        const r = baseRadius + bins[i] * amplitude + noise;
        coords.push([Math.cos(angle) * r, Math.sin(angle) * r]);
    }

    // Smooth closed path using cubic bezier curves
    let d = `M ${coords[0][0]} ${coords[0][1]}`;
    for (let i = 0; i < points; i++) {
        const curr = coords[i];
        const next = coords[(i + 1) % points];
        const prev = coords[(i - 1 + points) % points];
        const nextNext = coords[(i + 2) % points];

        const cp1x = curr[0] + (next[0] - prev[0]) / 6;
        const cp1y = curr[1] + (next[1] - prev[1]) / 6;
        const cp2x = next[0] - (nextNext[0] - curr[0]) / 6;
        const cp2y = next[1] - (nextNext[1] - curr[1]) / 6;

        d += ` C ${cp1x} ${cp1y}, ${cp2x} ${cp2y}, ${next[0]} ${next[1]}`;
    }
    d += 'Z';
    return d;
}
