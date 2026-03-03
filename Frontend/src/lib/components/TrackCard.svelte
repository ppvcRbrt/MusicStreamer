<script lang="ts">
    import {onMount} from "svelte";

    let { track, onclick }: { track?: App.Track, onclick?: (e: MouseEvent) => void } = $props();

    let containerRef: HTMLDivElement | null = $state(null);
    let textRef: HTMLSpanElement | null = $state(null);
    let shouldScroll = $state(false);

    $effect(() => {
        if (containerRef && textRef) {
            const ro = new ResizeObserver(() => {
                shouldScroll = textRef!.scrollWidth > containerRef!.clientWidth;
            });
            ro.observe(containerRef);
            return () => ro.disconnect();
        }
    });

    let duration = $derived(textRef ? textRef.scrollWidth / 30 : 10);
    let currentTrack = $derived(track ?? {
        id: 0,
        title: "Unknown Track",
        duration: 0,
        artist: {
            id: 0,
            name: "Unknown Artist"
        },
        album: {
            id: 0,
            title: "Unknown Album",
            image: null
        }
    });

</script>

<div class="flex flex-row w-full gap-3 items-center min-w-0 hover:cursor-pointer" {onclick}>
    <div class="flex-shrink-0">
        {#if currentTrack.album.image}
            <img src={currentTrack.album.image} class="rounded-lg" style="height: 3em; width: 3em;" />
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: 3em; width: 3em;" />
        {/if}
    </div>
    <div class="flex flex-col min-w-0 flex-1">
        <div class="marquee" bind:this={containerRef}>
            <div
                    class="marquee-track"
                    class:scrolling={shouldScroll}
                    style:animation-duration="{duration}s"
            >
                <span class="text-sm font-medium whitespace-nowrap text-start" bind:this={textRef}>
                    {currentTrack.title}
                </span>
                {#if shouldScroll}
                    <span class="separator text-sm opacity-30">•</span>
                    <span class="text-sm font-medium whitespace-nowrap text-start">
                        {currentTrack.title}
                    </span>
                {/if}
            </div>
        </div>
        <div class="flex flex-row gap-1 text-xs italic opacity-50">
            <span class="truncate">{currentTrack.artist.name}</span>
        </div>
    </div>
</div>

<style>
    .marquee {
        overflow: hidden;
        white-space: nowrap;
        width: 100%;
    }

    .marquee-track {
        display: inline-flex;
        align-items: center;
    }

    .separator {
        padding: 0 1rem;
    }

    .scrolling {
        animation: marquee linear infinite;
    }

    .scrolling:hover {
        animation-play-state: paused;
    }

    @keyframes marquee {
        0% {
            transform: translateX(0);
        }
        100% {
            transform: translateX(-50%);
        }
    }
</style>