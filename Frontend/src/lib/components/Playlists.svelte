<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {ChevronRight} from "@lucide/svelte";
    import {apiHttpService} from "$lib/services/apiHttpService";

    let { playlists, imageSize, height, title, Class="", onItemClick}:
        { playlists: App.Playlist[]|App.Album[], imageSize: string, height: string, title?: string, Class: string, onItemClick?: (itemType:App.Playlist|App.Album) => void} = $props();

    function onPlaylistClick(playlist: App.Playlist|App.Album) {
        if(onItemClick) {
            onItemClick(playlist);
        }
    }
</script>

{#snippet Playlist(playlist: App.Playlist|App.Album, first: boolean = false)}

    <Button variant="ghost" class="flex flex-row items-center justify-start p-2 w-full border-b rounded-none {first ? 'border-t' : ''}" size={imageSize} onclick={() => onPlaylistClick(playlist)}>
        {#if playlist.imageSmall}
            <img src={`${apiHttpService.getBaseUrl()}${playlist.imageSmall}`} alt={playlist.title} class="rounded-lg shrink-0" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            <div class="bg-gray-200 rounded-lg shrink-0" style="height: {imageSize}; width: {imageSize};" />
        {/if}
        <p class="text-sm font-medium truncate max-w-[60%]">{playlist.title}</p>
        <ChevronRight class="ml-auto shrink-0"/>
    </Button>
{/snippet}
<div class="flex flex-col justify-start {Class}">
    {#if title}
        <h1 class="text-3xl font-bold mb-1 ml-5">{title}</h1>
    {/if}
    <ScrollArea
            style="height: {height};"
            class="flex w-full rounded-md p-4"
            orientation="vertical">
        <div class="flex flex-col w-full justify-items-start">
            {#each playlists as playlist, index}
                {@render Playlist(playlist, index === 0)}
            {/each}
        </div>
    </ScrollArea>
</div>
