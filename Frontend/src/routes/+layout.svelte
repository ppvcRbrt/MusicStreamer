<script lang="ts">
    import './layout.css';
    import favicon from '$lib/assets/favicon.svg';
    import MusicPlayer from "$lib/components/MusicPlayer.svelte";
    let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
<div class="flex flex-col h-screen w-screen">
    <div class="flex-1 overflow-auto">
        {@render children()}
    </div>
    <div class="flex-shrink-0">
        <MusicPlayer/>
    </div>
</div>
