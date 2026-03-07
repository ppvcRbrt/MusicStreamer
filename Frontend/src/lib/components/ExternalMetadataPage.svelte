<script lang="ts">
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { Spinner } from "$lib/components/ui/spinner";
    import { Button } from "$lib/components/ui/button";
    import Artists from "$lib/components/Artists.svelte";
    import { ScrollArea } from "$lib/components/ui/scroll-area";
    import { Skeleton } from "$lib/components/ui/skeleton/index.js";
    import { CheckIcon } from "@lucide/svelte";
    import * as Dialog from "$lib/components/ui/dialog/index.js";
    interface SelectedArtist {
        artist: App.Artist;
        searchResult: App.MusicBrainzSearchResult;
    }
    let artistsMessage = $state("Disambiguate the artists using the MusicBrainz Results");
    let searchingArtists = $state(false);
    let artistPickerOpen = $state(false);
    let test = $state(true);
    let artists = $state<Record<number, App.MusicBrainzSearchResult>>({});
    let disambiguatedArtists = $state<Record<number, App.MusicBrainzArtist>>({});
    let selectedArtist = $state<SelectedArtist>();

    async function searchArtists() {
        searchingArtists = true;
        artists = await apiHttpService.get<Record<number, App.MusicBrainzSearchResult>>(`/externalMetadata/dbArtists`);
        searchingArtists = false;
    }

    async function artistDetails(artistId: number) {
        return await apiHttpService.get<App.Artist>(`/music/artist/${artistId}`);
    }

    function handleArtistClicked(artist: App.Artist) {
        selectedArtist = {
            artist: artist,
            searchResult: artists[artist.id]
        };
        artistPickerOpen = true;
    }
    function disambiguateArtist(artistId: number, artist: App.MusicBrainzArtist) {
        if(disambiguatedArtists[artistId]){
            delete disambiguatedArtists[artistId];
        }
        disambiguatedArtists[artistId] = artist;
    }

    async function submitDisambiguatedArtists() {
        let result = await apiHttpService.post<string>("/externalMetadata/disambiguateArtists", disambiguatedArtists);
        artists = {};
        disambiguatedArtists = {};
        artistsMessage = result;
    }
</script>

<div class="flex flex-col gap-2 mx-2">
    <Button variant="outline" onclick={searchArtists} disabled={searchingArtists}>
        {#if searchingArtists}
            <div class="flex w-full gap-2 justify-center items-center">
                <Spinner />
                <span>Searching For Artists In MusicBrainz...</span>
            </div>
        {:else}
            Search For Artists In MusicBrainz
        {/if}
    </Button>
    <ScrollArea
            class="flex w-full p-4 border"
            orientation="vertical"
            style="height: 200px;"
            >
        <div class="flex flex-col">
            {#if Object.keys(artists).length > 0}
                <p class="text-gray-400">{artistsMessage}</p>
                {#each Object.keys(artists) as artistId}
                    <div class="flex flex-col gap-1">
                        {#await artistDetails(artistId)}
                            <div class="flex w-full gap-2 justify-center items-center">
                                <Spinner />
                                <p class="text-sm">Loading artist details...</p>
                            </div>
                        {:then artist}
                            <Button variant="ghost" class="justify-start" onclick={()=>handleArtistClicked(artist)}>
                                {#if Object.keys(disambiguatedArtists).includes(artistId)}
                                    <CheckIcon class="text-green-500"/>
                                {/if}
                                {artist.name}
                            </Button>
                        {:catch error}
                            <p class="text-sm text-red-500">Error loading artist: {artistId} details</p>
                        {/await}
                    </div>
                {/each}
            {:else if searchingArtists}
                <div class="flex w-full gap-2 justify-center items-center">
                    <Spinner />
                    <p class="text-sm">Searching for artists...</p>
                </div>
                <div class="flex flex-col gap-2 mt-5 justify-center items-center">
                    <Skeleton class="h-25 w-full" />
                </div>
            {:else}
                 <p class="text-sm text-gray-500">No artists found. Try searching to see results.</p>
            {/if}
        </div>
    </ScrollArea>
    <Button variant="outline"
            onclick={submitDisambiguatedArtists}
            disabled={(Object.keys(disambiguatedArtists).length !== Object.keys(artists).length) || (Object.keys(artists).length === 0)}>
        Submit Disambiguated Artists
    </Button>
</div>

{#snippet ArtistPicker(artist: SelectedArtist)}
    <ScrollArea
            class="flex w-full p-4 border max-h-[60vh]"
            orientation="vertical"
    >
        <div class="flex flex-col gap-1">
            {#each artist.searchResult.artists as result}
                <div class="flex flex-row justify-between items-center min-w-0 overflow-hidden gap-1">
                    <Button
                            variant="outline"
                            class="flex-1 justify-start py-2 min-w-0 overflow-hidden
                                {disambiguatedArtists[artist.artist.id]?.id === result.id ? 'bg-primary/15!' : ''}"
                            onclick={()=>disambiguateArtist(artist.artist.id, result)}>
                        <div class="flex flex-col justify-start text-start min-w-0 w-full overflow-hidden">
                            <span class="truncate w-full">{result.name}</span>
                            <span class="text-xs text-gray-400 italic pb-1 break-all min-w-0 w-full overflow-hidden">
                                ({result.disambiguation})
                            </span>
                        </div>
                    </Button>
                    <Button variant="ghost" class="flex-shrink-0" onclick={() => window.open(result.musicBrainzUrl, '_blank')}>
                        Page
                    </Button>
                </div>
            {/each}
        </div>
    </ScrollArea>
{/snippet}

<Dialog.Root bind:open={artistPickerOpen}>
    <Dialog.Content
            class="flex flex-col max-w-2xl w-full max-h-[80vh]"
            interactOutsideBehavior="ignore"
            escapeKeydownBehavior="ignore">
        <Dialog.Header>
            <Dialog.Title>Select the correct artist from the MusicBrainz results</Dialog.Title>
            <Dialog.Description>
                {@render ArtistPicker(selectedArtist)}
                <Button variant="outline" class="mt-4" onclick={() => artistPickerOpen = false}>
                    Close
                </Button>
            </Dialog.Description>
        </Dialog.Header>
    </Dialog.Content>
</Dialog.Root>

