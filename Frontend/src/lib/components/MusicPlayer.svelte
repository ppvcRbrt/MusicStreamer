<script lang="ts">
    import { Button } from "$lib/components/ui/button/index.js";
    import { PlayIcon, ChevronLeftIcon, ChevronRightIcon, PauseIcon } from "@lucide/svelte";
    import TrackCard from "$lib/components/TrackCard.svelte";
    import { type MusicPlayerState, currentTrack } from "../../musicPlayerState.svelte";
    import { Progress } from "$lib/components/ui/progress";
    import { apiHttpService } from "$lib/services/apiHttpService";

    let playerState: MusicPlayerState = $state({
        track: null,
        isPlaying: false,
        currentTime: 0
    });
    currentTrack.subscribe(value => playerState = value);
    function togglePlay() {
        playerState.isPlaying = !playerState.isPlaying;
        currentTrack.set(playerState);
    }
    let audio: HTMLAudioElement|null = $state(null);
    let resourceUrl: string|null = $state(null);
    let currentTime = $state(0);
    let duration = $state(0);

    function onTimeUpdate() {
        currentTime = audio!.currentTime;
        playerState.currentTime = audio!.currentTime;
        currentTrack.set(playerState);
        navigator.mediaSession?.setPositionState({
            duration: audio!.duration || 0,
            playbackRate: audio!.playbackRate,
            position: audio!.currentTime
        });
    }

    function onLoadedMetadata() {
        duration = audio!.duration;
        setupMediaSession();
    }
    function setupMediaSession() {
        navigator.mediaSession.metadata = new MediaMetadata({
            title: playerState.track?.title,
            artist: playerState.track.artist.name,
            album: playerState.track.album?.title,
        });
        navigator.mediaSession.setActionHandler("play", () => audio!.play());
        navigator.mediaSession.setActionHandler("pause", () => audio!.pause());
        navigator.mediaSession.setActionHandler("seekbackward", () => audio.currentTime -= 10);
        navigator.mediaSession.setActionHandler("seekforward", () => audio.currentTime += 10);
    }

    $effect(() => {
        if (playerState.track) {
            resourceUrl = apiHttpService.getMediaResourceUrl(playerState.track!.filePath);
        }
    })

    $effect(() => {
        if (audio && playerState.isPlaying) {
            audio.play();
        }
        else {
            audio.pause();
        }
    });

</script>

<audio
        bind:this={audio}
        src={resourceUrl}
        ontimeupdate={onTimeUpdate}
        onloadedmetadata={onLoadedMetadata}
        oncanplay={() => {
        if (playerState.isPlaying) {
            audio.play();
        }
    }}
/>

{#if playerState.track && resourceUrl}
{/if}

<div class="flex flex-col mb-3">
    <div class="flex bg-slate-600 rounded-t-lg">
        <div class="flex flex-1 min-w-0 ml-3">
            <TrackCard track={playerState.track}/>
        </div>
        <div class="flex flex-shrink-0">
            <Button variant="ghost" class="rounded-2xl my-auto" size="icon" style="height: 3em; width: 3em;">
                <ChevronLeftIcon style="height: 1.3em; width: 1.3em"/>
            </Button>
            <Button variant="ghost" class="rounded-2xl my-auto" style="height: 5em; width: 5em;" onclick={togglePlay}>
                {#if playerState.isPlaying}
                    <PauseIcon style="height: 2em; width: 2em"/>
                {:else}
                    <PlayIcon style="height: 2em; width: 2em"/>
                {/if}
            </Button>
            <Button variant="ghost" class="rounded-2xl my-auto" style="height: 3em; width: 3em;">
                <ChevronRightIcon style="height: 1.3em; width: 1.3em"/>
            </Button>
        </div>
    </div>
    <div class="flex overflow-hidden">
        <Progress value={currentTime} max={duration} class="rounded-t-lg"/>
    </div>
</div>
