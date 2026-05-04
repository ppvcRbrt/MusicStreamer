<script lang="ts">
    import { Slider } from "$lib/components/ui/slider/index.js";
    import { type MusicPlayerState, playerState } from "../../musicPlayerState.svelte";
    import { SkipBackIcon, SkipForwardIcon, PauseIcon, PlayIcon, DiscAlbumIcon, ListMusicIcon } from "@lucide/svelte";
    import { playNext, playPrevious, togglePlay } from "../services/musicPlayerService.svelte";
    import { Button } from "$lib/components/ui/button";
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { Badge } from "$lib/components/ui/badge/index.js";
    import { userSettings } from "../../settingsState.svelte";
    import {closeSheet, openSheet, pageState, resetSheet, swipeState} from "../../bottomSheetState.svelte";
    import type { VerticalSpringSwipe } from "$lib/actions/verticalSpringSwipe.svelte";
    import { isNativePlatform } from "$lib/utils/platform";
    import {audioAnalyser, blobPath, extractDominantColor} from '$lib/utils/audioAnalyser.svelte';
    import { fade } from 'svelte/transition';
    import {ContextType, ListeningEventType} from "$lib/utils/enums.ts";
    import {logEvent} from "$lib/services/listeningEventService.ts";
    import {hapticMedium} from "$lib/utils/haptics.ts";

    let { progress = $bindable(), swipe }: { progress: number, swipe: VerticalSpringSwipe } = $props();

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
            logEvent(
                $playerState.playList[$playerState.trackIndex].id,
                value,
                $playerState.audioHandle.duration,
                value < $playerState.audioHandle.currentTime ? ListeningEventType.SeekBackward : ListeningEventType.SeekForward,
                ContextType.Player,
            ).catch(console.error);
            $playerState.audioHandle.currentTime = value;
            $playerState.isPlaying = true;
        }
    }
    async function goToAlbum() {
        if(currentTrack?.album) {
            let tracks = await apiHttpService.get<App.Track[]>(`/music/albums/${currentTrack.album.id}/tracks`)
            if(tracks.length > 0) {
                let album = { ...currentTrack.album, tracks: tracks.map(t => ({ ...t, artist: currentTrack!.artist, album: { ...currentTrack!.album, tracks: [] } })) };
                swipe.onSwipe("down");
                resetSheet();
                openSheet({
                    type: "album",
                    title: currentTrack!.album!.title,
                    items: album as App.Album
                });
            }
        }
    }

    function goToQueue() {
        if($playerState.playList.length > 0) {
            swipe.onSwipe("down");
            resetSheet();
            let playlist: App.Playlist = {
                id : -1,
                title: "Up Next",
                type: "playlist",
                tracks: $playerState.playList
            }
            openSheet({
                type: "queue",
                title: "Up Next",
                items: playlist
            });
        }
    }
    let lastLoggedTrackIndex = -1;
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
                    if($playerState.trackIndex !== lastLoggedTrackIndex){
                        lastLoggedTrackIndex = $playerState.trackIndex;
                        logEvent(
                            $playerState.playList[$playerState.trackIndex].id,
                            audio.currentTime,
                            audio.duration,
                            ListeningEventType.Resume,
                            ContextType.Player,
                        ).catch(console.error);
                    }
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
    $effect(()  => {
        const original = currentTrack?.album?.image;
        const large = currentTrack?.album?.imageLarge;
        const small = currentTrack?.album?.imageSmall;
        const base = apiHttpService.getBaseUrl();
        albumImageSrc = null; // reset on track change

        (async () => {
            if (await checkImageExists(base, large)) {
                albumImageSrc = `${base}${large}`;
            } else if (await checkImageExists(base, original)) {
                albumImageSrc = `${base}${original}`;
            } else if (await checkImageExists(base, small)) {
                albumImageSrc = `${base}${small}`;
            }
        })();
    });

    $effect(() => {
        if (albumImageSrc) {
            extractDominantColor(albumImageSrc);
        }
    });

    $effect(() => {
        audioAnalyser.isPaused = progress < 1;
    });
    async function checkImageExists(baseUrl: string, image): Promise<boolean> {
        return await apiHttpService.imageExists(`${baseUrl}${image}`)
    }

    function formatTime(seconds: number): string {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }

    function handlePlayPrevious() {
        hapticMedium();
        logEvent(
            $playerState.playList[$playerState.trackIndex].id,
            $playerState.audioHandle!.currentTime,
            $playerState.audioHandle!.duration,
            ListeningEventType.Skip,
            ContextType.Player,
        ).catch(console.error);
        playPrevious();
    }
    function handlePlayNext() {
        hapticMedium();
        logEvent(
            $playerState.playList[$playerState.trackIndex].id,
            $playerState.audioHandle!.currentTime,
            $playerState.audioHandle!.duration,
            ListeningEventType.Skip,
            ContextType.Player,
        );
        playNext();
    }

</script>

<div class="flex flex-col w-full justify-center items-center will-change-[filter]"
     style="opacity: {elementOpacity}; filter: blur({blur}px);">
    <div class="relative flex justify-center items-center overflow-visible"
         style="height: 16em; width: 16em; margin: 0 auto;"
         class:mt-3={isNativePlatform}>
        {#if !audioAnalyser.isPaused}
            {@const bins = audioAnalyser.bins}
            {@const t = audioAnalyser.time}

            <svg
                    class="absolute pointer-events-none"
                    style="z-index: -1; width: 32em; height: 32em; filter: blur(10px);"
                    viewBox="-1.5 -1.5 3 3"
            >
                <defs>
                    <radialGradient id="glow1">
                        <stop offset="0%" stop-color="rgba({audioAnalyser.glowColor}, 0.7)" />
                        <stop offset="70%" stop-color="rgba({audioAnalyser.glowColor}, 0.2)" />
                        <stop offset="100%" stop-color="rgba({audioAnalyser.glowColor}, 0)" />
                    </radialGradient>
                    <radialGradient id="glow2">
                        <stop offset="0%" stop-color="rgba({audioAnalyser.glowColorWarm}, 0.5)" />
                        <stop offset="70%" stop-color="rgba({audioAnalyser.glowColorWarm}, 0.15)" />
                        <stop offset="100%" stop-color="rgba({audioAnalyser.glowColorWarm}, 0)" />
                    </radialGradient>
                    <radialGradient id="glow3">
                        <stop offset="0%" stop-color="rgba({audioAnalyser.glowColorCool}, 0.4)" />
                        <stop offset="70%" stop-color="rgba({audioAnalyser.glowColorCool}, 0.1)" />
                        <stop offset="100%" stop-color="rgba({audioAnalyser.glowColorCool}, 0)" />
                    </radialGradient>
                </defs>

                <path
                        d={blobPath(bins, 1.1, t * 0.7, 0.4)}
                        fill="url(#glow1)"
                />
                <path
                        d={blobPath(bins.map((_, i) => bins[(i + 5) % 16]), 0.9, t * 1.1, 0.35)}
                        fill="url(#glow2)"
                />
                <path
                        d={blobPath(bins.map((_, i) => bins[(i + 10) % 16]), 0.7, t * 1.6, 0.3)}
                        fill="url(#glow3)"
                />
            </svg>
        {/if}

        {#if albumImageSrc}
            <img src={albumImageSrc} class="rounded-lg" style="height: 16em; width: 16em;" />
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: 16em; width: 16em;" />
        {/if}
    </div>
    <p class="text-lg font-medium mt-4">{currentTrack?.title ?? "Unknown Track"}</p>
    <p class="text-sm opacity-50">{currentTrack?.artist.name ?? "Unknown Artist"}</p>
    <Badge variant="outline" class="mt-2">{$userSettings.preferredAudioFormat}</Badge>
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
            <p class="text-xs opacity-50">{formatTime($playerState.duration)}</p>
        </div>
    </div>
    <div class="flex justify-center items-center gap-5 mt-4">
        <Button variant="ghost" class="rounded-2xl my-auto" size="icon" style="height: 3em; width: 3em;" onclick={handlePlayPrevious} disabled={$playerState.trackIndex <= 0}>
            <SkipBackIcon style="height: 2em; width: 2em"/>
        </Button>
        <Button variant="ghost" class="rounded-2xl my-auto" style="height: 5em; width: 5em;" onclick={()=>{togglePlay(); hapticMedium();}}>
            {#if $playerState.isPlaying}
                <PauseIcon style="height: 3em; width: 3em"/>
            {:else}
                <PlayIcon style="height: 3em; width: 3em"/>
            {/if}
        </Button>
        <Button variant="ghost" class="rounded-2xl my-auto" style="height: 3em; width: 3em;" onclick={handlePlayNext} disabled={$playerState.trackIndex >= $playerState.playList.length - 1}>
            <SkipForwardIcon style="height: 2em; width: 2em"/>
        </Button>
    </div>
    <div class="flex flex-row mt-5 justify-center items-center gap-2">
        <Button variant="ghost" class="rounded-2xl" onclick={goToAlbum}>
            <DiscAlbumIcon style="height: 2em; width: 2em"/>
            Album
        </Button>
        <Button variant="ghost" class="rounded-2xl" onclick={goToQueue}>
            <ListMusicIcon style="height: 2em; width: 2em"/>
            Queue
        </Button>

    </div>
</div>