<script lang="ts">
    import { ScrollArea } from "$lib/components/ui/scroll-area";
    import { Button } from "$lib/components/ui/button";
    import { ListMinusIcon, ListPlusIcon } from "@lucide/svelte";
    import { playerState } from "../../musicPlayerState.svelte";
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { HorizontalSpringSwipe } from "$lib/actions/horizontalSpringSwipe.svelte";
    import { swipeable } from '$lib/actions/gestures.svelte';
    import { flip } from "svelte/animate";
    import { slide } from "svelte/transition";
    import {hapticHeavy, hapticLight, hapticMedium} from "$lib/utils/haptics";


    let { tracks, imageSize, height, showTrackNumbers=false, isQueue=false }:
        { tracks: App.Track[], imageSize: string, height: string, showTrackNumbers?:boolean } = $props();
    let tracksOrdered = $state(tracks.sort((a, b) => a.trackNumber - b.trackNumber));
    let currentlyPlayingTrackId = $derived($playerState.playList[$playerState.trackIndex]?.id ?? -1);

    function onTrackClick(trackIndex: number) {
        if (tracksOrdered[trackIndex].id !== currentlyPlayingTrackId) {
            $playerState.playList = tracksOrdered;
            $playerState.trackIndex = trackIndex;
            $playerState.currentTime = 0;
            $playerState.isPlaying = true;
            hapticLight();
        }
        else {
            hapticHeavy();
        }
    }

    function handleRemoveAddFromQueue(track: App.Track) {
        if (isTrackInQueue(track)) {
            if (track.id === currentlyPlayingTrackId) {
                $playerState.isPlaying = false;
                $playerState.currentTime = 0;
            }

            const removedIndex = $playerState.playList.findIndex(t => t.id === track.id);
            $playerState.playList = $playerState.playList.filter(t => t.id !== track.id);

            if (removedIndex < $playerState.trackIndex) {
                $playerState.trackIndex = $playerState.trackIndex - 1;
            }

            if ($playerState.trackIndex >= $playerState.playList.length) {
                $playerState.trackIndex = Math.max(0, $playerState.playList.length - 1);
            }

            if (isQueue) {
                tracksOrdered = $playerState.playList;
            }
        }
        else {
            $playerState.playList = [...$playerState.playList, track];
        }
    }

    function isTrackInQueue(track: App.Track) {
        return $playerState.playList.some(t => t.id === track.id);
    }
</script>

{#snippet Track(track: App.Track, trackIndex: number)}
    {@const swipe = new HorizontalSpringSwipe(() => 100, () => 60)}
    <div class="border-b
                {trackIndex === 0 ? 'border-t rounded-none' : ''}">
        <div
                class="relative overflow-hidden"
                use:swipeable={{
                    axis: "horizontal",
                    onDrag: (_dy, dx) => {
                        const d = dx ?? 0;
                        swipe.onDrag(swipe.isRight ? Math.min(d, 0) : Math.max(d, 0));
                    },
                    onRelease: () => swipe.onRelease(),
                    handle: () => swipe.handle
        }}>
            <div class="flex items-center gap-0.5" style="transform: translateX({swipe.x.current}px)">
                <Button
                        variant="ghost"
                        size="icon"
                        class="flex-shrink-0 -ml-9 flex items-center justify-center shadow-2xl"
                        onclick={() => {
                            swipe.isRight = false;
                            swipe.x.set(0);
                            handleRemoveAddFromQueue(track);
                        }}
                >
                    {#if isTrackInQueue(track)}
                        <ListMinusIcon/>
                    {:else}
                        <ListPlusIcon/>
                    {/if}
                </Button>

                <div class="w-full">
                    <Button
                            variant="ghost"
                            class="flex flex-row items-center gap-2 py-6 w-full rounded-none
                                {track.id === currentlyPlayingTrackId ? 'bg-primary/10 hover:bg-primary/15 border-l-2 border-l-primary py-6' : ''}"
                            onclick={() => onTrackClick(trackIndex)}
                    >
                        {#if showTrackNumbers}
                            <span class="w-6 text-left opacity-50">{track.trackNumber}</span>
                        {:else}
                            {#if track.album.imageSmall}
                                <img src={`${apiHttpService.getBaseUrl()}${track.album.imageSmall}`} alt={track.title} class="rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};"/>
                            {:else}
                                <div class="bg-gray-200 rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};" />
                            {/if}
                        {/if}
                        <div class="flex flex-col items-start justify-start flex-1 min-w-0">
                            <p class="text-sm font-medium truncate w-full text-start">{track.title}</p>
                            <div class="flex flex-row gap-1 text-xs italic opacity-50">
                                <span class="truncate">{track.artist.name}</span>
                                <span>•</span>
                                <span class="truncate">{track.album.title}</span>
                            </div>
                        </div>
                    </Button>
                </div>
            </div>
        </div>
    </div>
{/snippet}

<ScrollArea
        style="height: {height};"
        class="flex w-full p-4"
        orientation="vertical">
        <div class="flex flex-col w-full justify-items-start">
            {#each tracksOrdered as track, index (track.id)}
                <div animate:flip={{ duration: 300 }} transition:slide={{ duration: 300 }}>
                    {@render Track(track, index)}
                </div>
            {/each}
        </div>
</ScrollArea>

