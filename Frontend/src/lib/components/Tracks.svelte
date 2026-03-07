<script lang="ts">
    import { ScrollArea } from "$lib/components/ui/scroll-area";
    import { Button } from "$lib/components/ui/button";
    import { ChevronRight } from "@lucide/svelte";
    import { playerState } from "../../musicPlayerState.svelte";

    let { tracks, imageSize, height, imageSrc, showTrackNumbers=false }:
        { tracks: App.Track[], imageSize: string, height: string, imageSrc?: string, showTrackNumbers?:boolean } = $props();
    let tracksOrdered = $state(tracks.sort((a, b) => a.trackNumber - b.trackNumber));
    let currentlyPlayingTrackId = $derived($playerState.playList[$playerState.trackIndex]?.id ?? -1);

    function onTrackClick(trackIndex: number) {
        $playerState.playList = tracksOrdered;
        $playerState.trackIndex = trackIndex;
        $playerState.currentTime = 0;
        $playerState.isPlaying = true;
    }

</script>

{#snippet Track(track: App.Track, trackIndex: number)}
    <Button
            variant="ghost"
            class="flex flex-row items-center gap-2 py-5 w-full border-b rounded-none
                    {trackIndex === 0 ? 'border-t rounded-none' : ''}
                    {track.id === currentlyPlayingTrackId ? 'bg-primary/10 hover:bg-primary/15 border-l-2 border-l-primary py-5' : ''}"
            onclick={() => onTrackClick(trackIndex)}>
        {#if showTrackNumbers}
            <span class="w-6 text-left opacity-50">{track.trackNumber}</span>
        {:else}
            {#if imageSrc}
                <img src={imageSrc} alt={track.title} class="rounded-full flex-shrink-0" style="height: {imageSize}; width: {imageSize};"/>
            {:else}
                <div class="bg-gray-200 rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};" />
            {/if}
        {/if}
        <div class="flex flex-col items-start justify-start flex-1 min-w-0 pb-1">
            <p class="text-sm font-medium truncate w-full text-start">{track.title}</p>
            <div class="flex flex-row gap-1 text-xs italic opacity-50">
                <span class="truncate">{track.artist.name}</span>
                <span>•</span>
                <span class="truncate">{track.album.title}</span>
            </div>
        </div>
<!--        <ChevronRight class="flex-shrink-0"/>-->
    </Button>
{/snippet}

<ScrollArea
        style="height: {height};"
        class="flex w-full rounded-md p-4"
        orientation="vertical">
    <div class="flex flex-col w-full justify-items-start">
        {#each tracksOrdered as track, index}
            {@render Track(track, index)}
        {/each}
    </div>
</ScrollArea>

