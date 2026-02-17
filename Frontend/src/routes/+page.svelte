<script lang="ts">
    import Artists from "$lib/components/Artists.svelte";
    import Playlists from "$lib/components/Playlists.svelte";

    let artists: App.Artist[] = [
        {
            id: 1,
            name: "Artist 1",
        },
        {
            id: 2,
            name: "Artist 2",
        },
        {
            id: 3,
            name: "Artist 3",
        },
        {
            id: 4,
            name: "Artist 4",
        },
        {
            id: 5,
            name: "Artist 5",
        },
        {
            id: 6,
            name: "Artist 6",
        },
    ];

    let playlists: App.Playlist[] = [
        {
            id: 1,
            name: "Playlist 1",
            songs: [],
            type: "playlist",
        },
        {
            id: 2,
            name: "Playlist 2",
            songs: [],
            type: "playlist",
        },
        {
            id: 3,
            name: "Playlist 3",
            songs: [],
            type: "playlist",
        },
        {
            id: 4,
            name: "Playlist 4",
            songs: [],
            type: "playlist",
        },
        {
            id: 5,
            name: "Playlist 5",
            songs: [],
            type: "playlist",
        },
        {
            id: 6,
            name: "Playlist 6",
            songs: [],
            type: "playlist",
        },
    ];
    let albums: App.Album[] = [
        {
            id: 1,
            name: "Album 1",
            songs: [],
            type: "album",
            artist: artists[0]
        },
        {
            id: 2,
            name: "Album 2",
            songs: [],
            type: "album",
            artist: artists[1]
        },
        {
            id: 3,
            name: "Album 3",
            songs: [],
            type: "album",
            artist: artists[2]
        },
        {
            id: 4,
            name: "Album 4",
            songs: [],
            type: "album",
            artist: artists[3]
        },
        {
            id: 5,
            name: "Album 5",
            songs: [],
            type: "album",
            artist: artists[4]
        },
        {
            id: 6,
            name: "Album 6",
            songs: [],
            type: "album",
            artist: artists[5]
        }
    ];
</script>
<Artists {artists} imageSize="8em"/>
<Playlists {playlists} imageSize="4em" height="300px"></Playlists>
<Playlists playlists={albums} imageSize="4em" height="300px"></Playlists>