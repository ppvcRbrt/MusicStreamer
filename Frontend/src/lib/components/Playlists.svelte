<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {ChevronRight, PlusIcon} from "@lucide/svelte";
    import {apiHttpService} from "$lib/services/apiHttpService";
    import * as Dialog from "$lib/components/ui/dialog/index.js";
    import {Input} from "$lib/components/ui/input";
    import {onMount} from "svelte";
    import {addedToPlaylistTrack, deletedPlaylistId} from "../../bottomSheetState.svelte.ts";

    let { playlists, imageSize, height, title, allowAdd=false, Class="", onItemClick, onPlaylistCreated }:
        { playlists: App.Playlist[]|App.Album[], imageSize: string, height: string, title?: string, allowAdd: boolean, Class: string, onItemClick?: (itemType:App.Playlist|App.Album) => void, onPlaylistCreated?: (playlist: App.Playlist) => void} = $props();
    let isDialogOpen = $state(false);
    let newPlaylistName = $state('');

    function onPlaylistClick(playlist: App.Playlist|App.Album) {
        if(onItemClick) {
            onItemClick(playlist);
        }
    }

    async function handleAddPlaylist() {
        if(newPlaylistName.trim() === '') return;
        let playlistStored: App.Playlist = await apiHttpService.post('/user/createPlaylist', {title: newPlaylistName, trackIds: []});
        if(onPlaylistCreated) {
            onPlaylistCreated(playlistStored);
        }
        isDialogOpen = false;
        newPlaylistName = '';
    }
    function getTrackCoversSmall(playlist: App.Playlist): string[] {
        const albumCounts = new Map<number, { image: string; count: number }>();

        for (const track of playlist.tracks) {
            if (!track.album?.id || !track.album?.imageSmall) continue;
            const entry = albumCounts.get(track.album.id);
            if (entry) {
                entry.count++;
            } else {
                albumCounts.set(track.album.id, { image: track.album.imageSmall, count: 1 });
            }
        }

        return [...albumCounts.values()]
            .sort((a, b) => b.count - a.count)
            .slice(0, 4)
            .map(e => e.image);
    }


    $effect(() => {
        const added = $addedToPlaylistTrack;
        if (added !== null) {
            const playlist = (playlists as App.Playlist[]).find(p => p.id === added.playlistId);
            if (playlist) {
                playlist.trackIds.push(added.track.id);
                added.track.trackNumber= playlist.trackIds.length;
                playlist.tracks.push(added.track);
                addedToPlaylistTrack.set(null);
            }
        }
    })

</script>

{#snippet Playlist(playlist: App.Playlist|App.Album, first: boolean = false)}

    <Button variant="ghost" class="flex flex-row items-center justify-start p-2 w-full border-b rounded-none {first ? 'border-t' : ''}" size={imageSize} onclick={() => onPlaylistClick(playlist)}>
        {#if playlist.imageSmall}
            <img src={`${apiHttpService.getBaseUrl()}${playlist.imageSmall}`} alt={playlist.title} class="rounded-lg shrink-0" style="height: {imageSize}; width: {imageSize};"/>
        {:else}
            {#if playlist.type === 'playlist'}
                {@const covers = getTrackCoversSmall(playlist)}
                <div class="rounded-lg overflow-hidden shrink-0 grid" style="height: {imageSize}; width: {imageSize}; grid-template-columns: {covers.length >= 2 ? '1fr 1fr' : '1fr'}; grid-template-rows: {covers.length >= 3 ? '1fr 1fr' : '1fr'};">
                    {#each covers as cover, i}
                        <!-- span the last cell if odd number (1 or 3 covers) -->
                        <img
                                src={`${apiHttpService.getBaseUrl()}${cover}`}
                                alt=""
                                class="w-full h-full object-cover"
                                style={covers.length === 3 && i === 2 ? 'grid-column: span 2' : covers.length === 1 ? 'grid-column: span 2; grid-row: span 2' : ''}
                        />
                    {/each}
                    {#if covers.length === 0}
                        <div class="bg-gray-200 w-full h-full" />
                    {/if}
                </div>
            {:else}
                <div class="bg-gray-200 rounded-lg shrink-0" style="height: {imageSize}; width: {imageSize};" />
            {/if}
        {/if}
        <p class="text-sm font-medium truncate max-w-[60%]">{playlist.title}</p>
        <ChevronRight class="ml-auto shrink-0"/>
    </Button>
{/snippet}
<div class="flex flex-col justify-start {Class}">
    {#if title}
        <div class="flex-row flex justify-start gap-4">
            <h1 class="text-3xl font-bold mb-1 ml-5">{title}</h1>
            {#if allowAdd}
                <Button variant="ghost" onclick={() => isDialogOpen = true}>
                    <PlusIcon/>
                </Button>
            {/if}
        </div>
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

<Dialog.Root bind:open={isDialogOpen}>
    <Dialog.Content>
        <Dialog.Header>
            <Dialog.Title>Create a new Playlist</Dialog.Title>
            <Dialog.Description class="m-2">
                <Input type="text" bind:value={newPlaylistName} placeholder="Playlist Name" class="w-full mb-2"/>
                <Button class="w-full mt-2" onclick={handleAddPlaylist}>Create</Button>
            </Dialog.Description>
        </Dialog.Header>
    </Dialog.Content>
</Dialog.Root>
