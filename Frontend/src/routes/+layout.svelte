<script lang="ts">
    import './layout.css';
    import favicon from '$lib/assets/favicon.svg';
    import MusicPlayer from "$lib/components/MusicPlayer.svelte";
    import BottomSheet from "$lib/components/BottomSheet.svelte";
    let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
<div class="flex flex-col h-screen w-screen relative">
    <div class="flex-1 overflow-auto pb-20">
        {@render children()}
    </div>
    <div class="fixed bottom-0 left-0 right-0">
        <MusicPlayer/>
        <BottomSheet/>
    </div>
</div>