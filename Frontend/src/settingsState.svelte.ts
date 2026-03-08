import { writable } from "svelte/store";

interface UserSettings {
    autoPreferredAudioFormat: boolean;
    preferredAudioFormat: 'flac' | 'opus';
}

const STORAGE_KEY = 'userSettings';
function loadPersistedState(): UserSettings {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) {
            return {
                autoPreferredAudioFormat: true,
                preferredAudioFormat: 'flac',
            };
        }
        return JSON.parse(raw) as UserSettings;
    }
    catch {
        return {
            autoPreferredAudioFormat: true,
            preferredAudioFormat: 'flac',
        }
    }
}

function savePersistedState(state: UserSettings) {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
        // storage might be unavailable
    }
}
const persisted = loadPersistedState();

export const userSettings = writable<UserSettings>({
    autoPreferredAudioFormat: persisted.autoPreferredAudioFormat,
    preferredAudioFormat: persisted.preferredAudioFormat,
});

userSettings.subscribe((state) => {
    savePersistedState(state);
});