import {type MusicPlayerState, playerState} from "../../musicPlayerState.svelte";

let playerStateValue: MusicPlayerState;
playerState.subscribe(value => {
    playerStateValue = value;
});
export function togglePlay() {
    if (playerStateValue.audioHandle) {
        if (playerStateValue.isPlaying) {
            playerStateValue.audioHandle.pause();
            playerState.update(state => ({ ...state, isPlaying: false }));
        } else {
            playerStateValue.audioHandle.play();
            playerState.update(state => ({ ...state, isPlaying: true }));
        }
    }
}

export function playNext() {
    if (playerStateValue.playList.length > 0) {
        const nextIndex = (playerStateValue.trackIndex + 1) % playerStateValue.playList.length;
        playerState.update(state => ({ ...state, trackIndex: nextIndex, isPlaying: true }));
    }
}

export function playPrevious() {
    if (playerStateValue.playList.length > 0) {
        const prevIndex = (playerStateValue.trackIndex - 1 + playerStateValue.playList.length) % playerStateValue.playList.length;
        playerState.update(state => ({...state, trackIndex: prevIndex, isPlaying: true}));
    }
}




