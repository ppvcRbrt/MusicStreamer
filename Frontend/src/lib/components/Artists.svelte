<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {openSheet} from "../../bottomSheetState.svelte";
    import {apiHttpService} from "$lib/services/apiHttpService";
    let { artists, imageSize, title="Artists", Class="", onArtistClicked }:
        { artists: App.Artist[], imageSize: string, title: string, Class: string, onArtistClicked: (artist:App.Artist) => void } = $props();

    let artistImageSrc = $state<Record<string, string | null>>({});

    function onClicked(artist: App.Artist) {
        onArtistClicked(artist);
    }

    $effect(() => {
        if (artists) {
            const base = apiHttpService.getBaseUrl();

            for (const artist of artists) {
                const large = artist.imageLarge;
                const small = artist.imageSmall;

                artistImageSrc[artist.id] = null;

                if (large) {
                    apiHttpService.imageExists(`${base}${large}`).then(exists => {
                        artistImageSrc[artist.id] = exists ? `${base}${large}` : (small ? `${base}${small}` : null);
                    });
                } else if (small) {
                    artistImageSrc[artist.id] = `${base}${small}`;
                }
            }
        }
    });
</script>

{#snippet Artist(artist: App.Artist)}
    <Button variant="ghost" class="flex flex-col items-center p-2" onclick={() => onClicked(artist)} size={imageSize}>
        {#if artistImageSrc[artist.id]}
            <img src={artistImageSrc[artist.id]} alt={artist.name} class="rounded-full" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: {imageSize}; width: {imageSize};" />
        {/if}
        <p class="text-sm font-medium text-center">{artist.name}</p>
    </Button>
{/snippet}
<div class="flex flex-col justify-start {Class}">
    <h1 class="text-3xl font-bold mb-1 ml-5">{title}</h1>
    <ScrollArea
            class="flex w-full p-4"
            orientation="horizontal">
        <div class="flex w-max gap-4">
            {#each artists as artist}
                {@render Artist(artist)}
            {/each}
        </div>
    </ScrollArea>
</div>

