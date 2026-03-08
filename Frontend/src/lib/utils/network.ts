type AudioQuality = 'flac' | 'opus';

interface ConnectionInfo {
    type: string;
    effectiveType: string;
    saveData: boolean;
}

export function getConnectionInfo(): ConnectionInfo {
    const conn = (navigator as any).connection;
    if (!conn) {
        return { type: 'unknown', effectiveType: 'unknown', saveData: false };
    }

    return {
        type: conn.type ?? 'unknown',
        effectiveType: conn.effectiveType ?? 'unknown',
        saveData: conn.saveData ?? false,
    };
}

export function isMobileDevice(): boolean {
    return /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)
        || (navigator.maxTouchPoints > 0 && window.innerWidth < 768);
}

export function getPreferredAudioQuality(): AudioQuality {
    const { type, effectiveType, saveData } = getConnectionInfo();

    if (saveData) return 'opus';
    if (type === 'cellular') return 'opus';
    if (/slow-2g|2g|3g/.test(effectiveType)) return 'opus';
    if (isMobileDevice()) return 'opus';

    return 'flac';
}

export function onConnectionChange(callback: (quality: AudioQuality) => void): () => void {
    const conn = (navigator as any).connection;
    if (!conn) return () => {};

    const handler = () => callback(getPreferredAudioQuality());
    conn.addEventListener('change', handler);

    return () => conn.removeEventListener('change', handler);
}


