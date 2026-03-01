<script lang="ts">
    import Artists from "$lib/components/Artists.svelte";
    import Playlists from "$lib/components/Playlists.svelte";
    import type { PageProps } from './$types';
    import Tracks from "$lib/components/Tracks.svelte";

    let { data }: { data: PageProps } = $props();
</script>
<!--<Artists artists={data.artists} imageSize="8em"/>-->
<!--<Playlists playlists={data.albums} imageSize="4em" height="300px"></Playlists>-->
<Tracks tracks={data.tracks} imageSize="2em" height="100%"></Tracks>