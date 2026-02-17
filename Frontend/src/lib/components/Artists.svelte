<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    let { artists, imageSize }: { artists: App.Artist[], imageSize: string } = $props();

    function onArtistClick(artist: App.Artist) {
        // Handle artist click, e.g., navigate to artist's page or show details
        console.log("Clicked artist:", artist.id);
    }
</script>

{#snippet Artist(artist: App.Artist)}
    <Button variant="ghost" class="flex flex-col items-center p-2" onclick={() => onArtistClick(artist)} size={imageSize}>
        {#if artist.image}
            <img src={artist.image} alt={artist.name} class="rounded-full" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: {imageSize}; width: {imageSize};" />
        {/if}
        <p class="text-sm font-medium text-center">{artist.name}</p>
    </Button>
{/snippet}
<ScrollArea
        class="flex w-full p-4"
        orientation="horizontal">
    <div class="flex w-max gap-4">
        {#each artists as artist}
            {@render Artist(artist)}
        {/each}
    </div>
</ScrollArea>

