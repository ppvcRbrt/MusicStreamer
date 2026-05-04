import {type MusicPlayerState, playerState} from "../../musicPlayerState.svelte";
import {logEvent} from "$lib/services/listeningEventService";
import {ContextType, ListeningEventType} from "$lib/utils/enums";

let playerStateValue: MusicPlayerState;
playerState.subscribe(value => {
    playerStateValue = value;
});
export function togglePlay() {
    const audio = playerStateValue.audioHandle;
    if (audio) {
        if (!audio.paused) {
            audio.pause();
            playerState.update(state => ({ ...state, isPlaying: false}));
            logEvent(
                playerStateValue.playList[playerStateValue.trackIndex].id,
                playerStateValue.audioHandle?.currentTime ?? 0,
                playerStateValue.audioHandle?.duration ?? 0,
                ListeningEventType.Pause,
                ContextType.Player
            ).catch(console.error);
        } else {
            audio.play();
            playerState.update(state => ({ ...state, isPlaying: true }));
            logEvent(
                playerStateValue.playList[playerStateValue.trackIndex].id,
                playerStateValue.audioHandle?.currentTime ?? 0,
                playerStateValue.audioHandle?.duration ?? 0,
                ListeningEventType.Resume,
                ContextType.Player
            ).catch(console.error);
        }
    }
}

export async function playNext() {
    if (playerStateValue.playList.length > 0) {
        const nextIndex = (playerStateValue.trackIndex + 1) % playerStateValue.playList.length;
        playerState.update(state => ({ ...state, trackIndex: nextIndex, isPlaying: true, currentTime: 0}));
    }
}

export async function playPrevious() {
    if (playerStateValue.playList.length > 0) {
        const prevIndex = (playerStateValue.trackIndex - 1 + playerStateValue.playList.length) % playerStateValue.playList.length;
        playerState.update(state => ({...state, trackIndex: prevIndex, isPlaying: true, currentTime: 0}));
    }
}




