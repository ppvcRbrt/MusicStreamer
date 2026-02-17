<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {ChevronRight} from "@lucide/svelte";

    let { playlists, imageSize, height }: { playlists: App.Playlist[]|App.Album[], imageSize: string, height: string } = $props();

    function onPlaylistClick(playlist: App.Playlist|App.Album) {
        switch (playlist.type) {
            case "playlist":
                console.log("Clicked Playlist", playlist.id);
                break;
            case "album":
                console.log("Clicked Album", playlist.id);
                break;
            default:
                console.warn("Unknown playlist type:", playlist.type);
        }
    }
</script>

{#snippet Playlist(playlist: App.Playlist|App.Album, first: boolean = false)}

    <Button variant="ghost" class="flex flex-row items-center justify-start p-2 w-full border-b rounded-none {first ? 'border-t' : ''}" size={imageSize} onclick={() => onPlaylistClick(playlist)}>
        {#if playlist.image}
            <img src={playlist.image} alt={playlist.name} class="rounded-full" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: {imageSize}; width: {imageSize};" />
        {/if}
        <p class="text-sm font-medium text-center">{playlist.name}</p>
        <ChevronRight class="ml-auto"/>
    </Button>
{/snippet}
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

