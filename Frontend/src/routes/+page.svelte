<script lang="ts">
    import Fuse from 'fuse.js';
    import Artists from "$lib/components/Artists.svelte";
    import Playlists from "$lib/components/Playlists.svelte";
    import type { PageProps } from './$types';
    import Search from "$lib/components/Search.svelte";
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {openSheet} from "../bottomSheetState.svelte";

    let { data }: { data: PageProps } = $props();
    let filteredTracks: App.Track[] = $state(data.tracks);
    let filteredAlbums: App.Album[] = $state(data.albums);
    let filteredArtists: App.Artist[] = $state(data.artists);
    let searchQuery = $state("");
    let scrolled = $state(false);
    let currentSection = $state("My Library");

    const trackFuse = new Fuse(data.tracks, { keys: ['title', 'artist.name', 'album.title'], minMatchCharLength: 1, includeScore: true, shouldSort: true, distance: 5 });
    const albumFuse = new Fuse(data.albums, { keys: ['title', 'artist.name'], ignoreLocation: true, minMatchCharLength: 2, includeScore: true });
    const artistFuse = new Fuse(data.artists, { keys: ['name'], ignoreLocation: true, minMatchCharLength: 2, includeScore: true });

    $effect(() => {
        if (searchQuery.length > 0) {
            filteredTracks = trackFuse.search(searchQuery).map(r => r.item) as App.Track[];
            filteredAlbums = albumFuse.search(searchQuery).map(r => r.item) as App.Album[];
            filteredArtists = artistFuse.search(searchQuery).map(r => r.item) as App.Artist[];
        } else {
            filteredTracks = data.tracks;
            filteredAlbums = data.albums;
            filteredArtists = data.artists;
        }
    });

    function handleScroll(e: Event) {
        const target = e.target as HTMLElement;
        scrolled = target.scrollTop > 40;
    }

    function handleArtistClicked(artist: App.Artist) {
        openSheet({
            type: "artist",
            title: artist.name,
            items: artist
        })
    }

    function handleAlbumClicked(album: App.Album) {
        let albumTracks = album.tracks.map(t => {
            return { ...t, artist: album.artist, album: { ...album, tracks: [] } }
        });
        let albumWithTracks = { ...album, tracks: albumTracks };
        openSheet({
            type: "album",
            title: album.title,
            items: albumWithTracks
        })
    }
    function observeSection(node: HTMLElement, sectionName: string) {
        const viewport = document.querySelector('[data-radix-scroll-area-viewport]');
        const heading = node;

        const observer = new IntersectionObserver(
            ([entry]) => { if (entry.isIntersecting) currentSection = sectionName; },
            { root: viewport, rootMargin: '0px 0px -90% 0px', threshold: 0 }
        );

        observer.observe(heading);
        return { destroy: () => observer.disconnect() };
    }
</script>

<ScrollArea class="h-full" onscrollcapture={handleScroll}>
    <div class="sticky top-0 z-10 transition-all duration-300 bg-secondary/40 backdrop-blur-2xl
                {scrolled ? 'py-2 shadow-md' : 'py-0 pointer-events-none opacity-0 h-0'}">
        <div class="flex items-center gap-3 px-4">
            <span class="font-semibold text-sm truncate flex-1">{currentSection}</span>
            <div class="w-[70%]">
                <Search bind:searchQuery />
            </div>
        </div>
    </div>

    <div class="flex justify-center my-4 transition-all duration-300
                {scrolled ? 'opacity-0 h-0 overflow-hidden mt-0' : 'opacity-100'}">
        <div class="sm:w-[50%] w-full px-2">
            <Search bind:searchQuery Class="backdrop-blur-3xl bg-secondary/50"/>
        </div>
    </div>

    <div id="artists-main" use:observeSection={"Artists"}>
        <Artists artists={filteredArtists} imageSize="8em" Class="mt-2" onArtistClicked={handleArtistClicked}/>
    </div>
    <div id="albums-main" use:observeSection={"Albums"}>
        <Playlists playlists={filteredAlbums} title="Albums" imageSize="4em" Class="mt-5" onItemClick={handleAlbumClicked}/>
    </div>
</ScrollArea>