<script lang="ts">
    import { Slider } from "$lib/components/ui/slider/index.js";
    import { type MusicPlayerState, playerState } from "../../musicPlayerState.svelte";
    import { SkipBackIcon, SkipForwardIcon, PauseIcon, PlayIcon } from "@lucide/svelte";
    import { playNext, playPrevious, togglePlay } from "../services/musicPlayerService.svelte";
    import { Button } from "$lib/components/ui/button";
    import {apiHttpService} from "$lib/services/apiHttpService";

    let { progress = $bindable()}: { progress: number} = $props();

    let currentTrack = $derived<App.Track>($playerState.playList[$playerState.trackIndex]);
    let blur = $derived(Math.max(10 - progress * 10, 0)); // starts at 10, goes to 0
    let elementOpacity = $derived(progress < 0.4 ? 0 :Math.min((progress - 0.4) * (1 / 0.6), 1)); // hidden until 40%, then fades in
    let isDragging = $state(false);
    let displayTime = $state(0);
    let isSeeking = $state(false);
    let previousTrackIndex = $state($playerState.trackIndex);
    let albumImageSrc = $state<string | null>(null);

    function startedDragging() {
        isDragging = true;
    }
    function stoppedDragging(value: number) {
        displayTime = value;
        isSeeking = true;
        isDragging = false;
        // $playerState.audioHandle?.pause();
    }
    function clickedSlider(value: number) {
        displayTime = value;
        isSeeking = true;
        $playerState.audioHandle?.pause();
    }
    function setSliderValue(value: number) {
        if(isDragging) {
            displayTime = value;
        }
    }
    function setAudioValue(value: number) {
        if($playerState.audioHandle) {
            $playerState.audioHandle.currentTime = value;
            $playerState.isPlaying = true;
        }
    }

    $effect(() => {
        const audio = $playerState.audioHandle;
        if (!audio) return;
        const handleSeeked = async () => {
            isSeeking = false;

            // Wait for enough data to be buffered
            if (audio.readyState < 3) { // HAVE_FUTURE_DATA
                await new Promise(resolve => {
                    audio.addEventListener('canplay', resolve, { once: true });
                });
            }

            try {
                if($playerState.isPlaying)
                {
                    await audio.play();
                }
            } catch (error) {
                console.error('Failed to resume playback after seek:', error);
                // Optionally retry or update UI to show play button
            }
        };
        audio.addEventListener('seeked', handleSeeked);
        return () => {
            audio.removeEventListener('seeked', handleSeeked);
        };
    });
    $effect(() => {
        if (!isDragging && !isSeeking) {
            displayTime = $playerState?.audioHandle?.currentTime ?? 0;
        }
    });
    $effect(() => {
        const currentIndex = $playerState.trackIndex;
        if (currentIndex !== previousTrackIndex) {
            previousTrackIndex = currentIndex;
            isDragging = false;
            isSeeking = false;
            displayTime = 0;
            console.log(currentTrack.album.image);
        }
    });
    $effect(() => {
        const large = currentTrack?.album?.imageLarge;
        const small = currentTrack?.album?.image;
        const base = apiHttpService.getBaseUrl();

        albumImageSrc = null; // reset on track change

        if (large) {
            imageExists(`${base}${large}`).then(exists => {
                albumImageSrc = exists ? `${base}${large}` : (small ? `${base}${small}` : null);
            });
        } else if (small) {
            albumImageSrc = `${base}${small}`;
        }
    });
    function formatTime(seconds: number): string {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
    async function imageExists(path: string): Promise<boolean> {
        const res = await fetch(path, { method: "HEAD" });
        if (res.ok) {
            return res.ok;
        }
    }
</script>

<div class="flex flex-col w-full justify-center items-center" style="opacity: {elementOpacity}; filter: blur({blur}px);">
    <div>
        {#if albumImageSrc}
            <img src={albumImageSrc} class="rounded-lg" style="height: 16em; width: 16em;" />
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: 16em; width: 16em;" />
        {/if}
    </div>
    <p class="text-lg font-medium mt-4">{currentTrack?.title ?? "Unknown Track"}</p>
    <p class="text-sm opacity-50">{currentTrack?.artist.name ?? "Unknown Artist"}</p>
    <div class="flex flex-col w-full justify-center items-center">
        <Slider
                class="w-11/12 mt-7 **:data-[slot=slider-track]:h-2 **:data-[slot=slider-thumb]:size-5"
                type="single"
                value={displayTime}
                onValueChange={(value) => setSliderValue(value)}
                onValueCommit={setAudioValue}
                onDragStart={startedDragging}
                onDragEnd={stoppedDragging}
                onSliderClick={clickedSlider}
                max={$playerState.duration ?? 0}
                step={0.1}
        />
        <div class="flex flex-row w-full justify-between mt-4 px-5">
            <p class="text-xs opacity-50">{formatTime(displayTime)}</p>
            <p class="text-xs opacity-50">{formatTime($playerState.duration - displayTime)}</p>
        </div>
    </div>
    <div class="flex justify-center items-center gap-5 mt-4">
        <Button variant="ghost" class="rounded-2xl my-auto" size="icon" style="height: 3em; width: 3em;" onclick={playPrevious} disabled={$playerState.trackIndex <= 0}>
            <SkipBackIcon style="height: 2em; width: 2em"/>
        </Button>
        <Button variant="ghost" class="rounded-2xl my-auto" style="height: 5em; width: 5em;" onclick={togglePlay}>
            {#if $playerState.isPlaying}
                <PauseIcon style="height: 3em; width: 3em"/>
            {:else}
                <PlayIcon style="height: 3em; width: 3em"/>
            {/if}
        </Button>
        <Button variant="ghost" class="rounded-2xl my-auto" style="height: 3em; width: 3em;" onclick={playNext} disabled={$playerState.trackIndex >= $playerState.playList.length - 1}>
            <SkipForwardIcon style="height: 2em; width: 2em"/>
        </Button>
    </div>
</div>