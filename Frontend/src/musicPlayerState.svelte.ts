import { writable } from 'svelte/store';

export type MusicPlayerState = {
    track: App.Track | null;
    isPlaying: boolean;
    currentTime: number;
};

export const currentTrack = writable<MusicPlayerState>({
    track: null,
    isPlaying: false,
    currentTime: 0,
});
