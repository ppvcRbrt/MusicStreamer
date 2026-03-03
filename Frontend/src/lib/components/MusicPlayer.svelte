<script lang="ts">
    import { Button } from "$lib/components/ui/button/index.js";
    import { PlayIcon, ChevronLeftIcon, ChevronRightIcon, PauseIcon } from "@lucide/svelte";
    import TrackCard from "$lib/components/TrackCard.svelte";
    import { type MusicPlayerState, playerState} from "../../musicPlayerState.svelte";
    import { Progress } from "$lib/components/ui/progress";
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { swipeable } from '$lib/actions/gestures.svelte';
    import MusicPlayerPage from "$lib/components/MusicPlayerPage.svelte";
    import {VerticalSpringSwipe} from "$lib/actions/verticalSpringSwipe.svelte";
    import {playNext, playPrevious, togglePlay} from "$lib/services/musicPlayerService.svelte";
    import { browser } from '$app/environment';

    let playerHeight = $state(0);
    let windowInnerHeight = $state(browser ? window.innerHeight : 0);
    const swipe = new VerticalSpringSwipe(
        () => windowInnerHeight,
        () => playerHeight
    );
    let blur = $derived(Math.min(swipe.progress * 10, 10));
    let elementOpacity = $derived(Math.max(1 - swipe.progress * 2, 0));

    let currentTrack = $derived<App.Track>($playerState.playList[$playerState.trackIndex]);
    let resourceUrl = $derived(apiHttpService.getMediaResourceUrl(currentTrack?.filePath ?? ''));
    let currentTime = $derived($playerState.currentTime);
    let duration = $state(0);

    function onTimeUpdate() {
        currentTime = $playerState.audioHandle!.currentTime;
        $playerState.currentTime = $playerState.audioHandle!.currentTime;
        navigator.mediaSession?.setPositionState({
            duration: $playerState.audioHandle!.duration || 0,
            playbackRate: $playerState.audioHandle!.playbackRate,
            position: $playerState.audioHandle!.currentTime
        });
    }
    function onLoadedMetadata() {
        duration = $playerState.audioHandle!.duration;
        $playerState.duration = $playerState.audioHandle!.duration;
        setupMediaSession();
    }
    function setupMediaSession() {
        navigator.mediaSession.metadata = new MediaMetadata({
            title: currentTrack?.title,
            artist: currentTrack.artist.name,
            album: currentTrack.album?.title,
        });
        navigator.mediaSession.setActionHandler("play", () => { $playerState.audioHandle!.play(); playerState.update(s => ({ ...s, isPlaying: true })); });
        navigator.mediaSession.setActionHandler("pause", () => { $playerState.audioHandle!.pause(); playerState.update(s => ({ ...s, isPlaying: false })); });
        navigator.mediaSession.setActionHandler("seekbackward", () => $playerState.audioHandle.currentTime -= 10);
        navigator.mediaSession.setActionHandler("seekforward", () => $playerState.audioHandle.currentTime += 10);
    }

    $effect(() => {
        document.body.style.overflow = swipe.isUp ? 'hidden' : '';
    });

</script>

<audio
        bind:this={$playerState.audioHandle}
        src={resourceUrl}
        ontimeupdate={onTimeUpdate}
        onloadedmetadata={onLoadedMetadata}
        onended={playNext}
        onplay={() => playerState.update(s => ({ ...s, isPlaying: true }))}
        onpause={() => playerState.update(s => ({ ...s, isPlaying: false }))}
        oncanplay={() => {
        if ($playerState.isPlaying) {
            $playerState.audioHandle.play();
        }
    }}
/>

<div
        use:swipeable={{
        onDrag: (dy) => swipe.onDrag(dy),
        onRelease: () => swipe.onRelease(),
        handle: () => swipe.handle
    }}
        style="transform: translateY({swipe.y.current}px); margin-bottom: -{windowInnerHeight - playerHeight + 25}px;"
>
    <div class="flex h-1.5 justify-center">
        <div class="w-[42.5%] border-b"></div>
        <div class="w-[15%] border-t rounded-t-xl backdrop-blur-xl bg-secondary/70"></div>
        <div class="w-[42.5%] border-b"></div>
    </div>
    <div class="backdrop-blur-xl bg-secondary/40">
        <div class="music-player" bind:this={swipe.handle} bind:clientHeight={playerHeight}>
            <div class="flex" style="opacity: {elementOpacity};">
                <div class="flex flex-1 min-w-0 ml-3" style="filter: blur({blur}px);">
                    <TrackCard track={currentTrack}/>
                </div>
                <div class="flex flex-shrink-0" style="filter: blur({blur}px);">
                    <Button variant="ghost" class="rounded-2xl my-auto" size="icon" style="height: 3em; width: 3em;" onclick={playPrevious} disabled={$playerState.trackIndex <= 0}>
                        <ChevronLeftIcon style="height: 1.3em; width: 1.3em"/>
                    </Button>
                    <Button variant="ghost" class="rounded-2xl my-auto" style="height: 5em; width: 5em;" onclick={togglePlay}>
                        {#if $playerState.isPlaying}
                            <PauseIcon style="height: 2em; width: 2em"/>
                        {:else}
                            <PlayIcon style="height: 2em; width: 2em"/>
                        {/if}
                    </Button>
                    <Button variant="ghost" class="rounded-2xl my-auto" style="height: 3em; width: 3em;" onclick={playNext} disabled={$playerState.trackIndex >= $playerState.playList.length - 1}>
                        <ChevronRightIcon style="height: 1.3em; width: 1.3em"/>
                    </Button>
                </div>
            </div>
            <div class="flex overflow-hidden">
                <Progress value={currentTime} max={duration} class="rounded-t-3xl" style="opacity: {elementOpacity};" />
            </div>
        </div>
        <div style="height: {windowInnerHeight - playerHeight + 25}px;">
            <MusicPlayerPage bind:progress={swipe.progress}/>
        </div>
    </div>
</div>
