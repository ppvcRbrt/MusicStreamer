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

export const playerState = writable<MusicPlayerState>({
    trackIndex: -1,
    isPlaying: false,
    currentTime: 0,
    duration: 0,
    playList: [],
});
