import { writable } from 'svelte/store';

export type MusicPlayerState = {
    // track: App.Track | null;
    trackIndex: number;
    isPlaying: boolean;
    currentTime: number;
    duration: number;
    playList: App.Track[];
    audioHandle?: HTMLAudioElement | null;
};
type PersistedState = Pick<MusicPlayerState, 'trackIndex' | 'playList' | 'currentTime' >;
const STORAGE_KEY = 'musicPlayerState';

function loadPersistedState(): Partial<PersistedState> {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return {};
        let persistedState = JSON.parse(raw) as PersistedState;
        return persistedState;
    } catch {
        return {};
    }
}

const persisted = loadPersistedState();

function savePersistedState(state: MusicPlayerState) {
    try {
        const toSave: PersistedState = {
            trackIndex: state.trackIndex,
            playList: state.playList,
            currentTime: state.currentTime,
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(toSave));
    } catch {
        // storage might be unavailable
    }
}

export const playerState = writable<MusicPlayerState>({
    trackIndex: persisted.trackIndex ?? -1,
    isPlaying: false, // never restore as playing
    currentTime: persisted.currentTime ?? 0,
    duration: 0,
    playList: persisted.playList ?? [],
});

playerState.subscribe((state) => {
    savePersistedState(state);
});