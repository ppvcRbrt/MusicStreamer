<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {ChevronRight} from "@lucide/svelte";
    import {type MusicPlayerState, currentTrack } from "../../musicPlayerState.svelte";

    let { tracks, imageSize, height, imageSrc }: { tracks: App.Track[], imageSize: string, height: string, imageSrc?: string } = $props();
    let playerState: MusicPlayerState = $state({
        track: null,
        isPlaying: false,
        currentTime: 0
    });
    currentTrack.subscribe(value => playerState = value);

    function onTrackClick(track: App.Track) {
        playerState.track = track;
        playerState.isPlaying = true;
        currentTrack.set(playerState);
    }
</script>

{#snippet Track(track: App.Track, first: boolean = false)}
    <Button variant="ghost" class="flex flex-row items-center gap-2 p-2 w-full border-b rounded-none {first ? 'border-t' : ''}" onclick={() => onTrackClick(track)}>
        {#if imageSrc}
            <img src={imageSrc} alt={track.title} class="rounded-full flex-shrink-0" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            <div class="bg-gray-200 rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};" />
        {/if}
        <div class="flex flex-col items-start justify-start flex-1 min-w-0">
            <p class="text-sm font-medium truncate w-full text-start">{track.title}</p>
            <div class="flex flex-row gap-1 text-xs italic opacity-50">
                <span class="truncate">{track.artist.name}</span>
                <span>•</span>
                <span class="truncate">{track.album.title}</span>
            </div>
        </div>
        <ChevronRight class="flex-shrink-0"/>
    </Button>
{/snippet}

<ScrollArea
        style="height: {height};"
        class="flex w-full rounded-md p-4"
        orientation="vertical">
    <div class="flex flex-col w-full justify-items-start gap-1">
        {#each tracks as track, index}
            {@render Track(track, index === 0)}
        {/each}
    </div>
</ScrollArea>

