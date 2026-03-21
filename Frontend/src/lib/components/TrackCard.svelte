<script lang="ts">
    import {apiHttpService} from "$lib/services/apiHttpService";

    let { track, onclick }: { track?: App.Track, onclick?: (e: MouseEvent) => void } = $props();

    let containerRef: HTMLDivElement | null = $state(null);
    let textRef: HTMLSpanElement | null = $state(null);
    let separatorRef: HTMLSpanElement | null = $state(null);
    let shouldScroll = $state(false);
    let animation: Animation | null = null;

    function startAnimation() {
        if (!textRef || !separatorRef) return;
        animation?.cancel();

        // Exact scroll distance: first title width + separator width
        const scrollDistance = textRef.scrollWidth + separatorRef.scrollWidth;
        const duration = (scrollDistance / 40) * 1000; // ms, 40px/s
        const pauseRatio = Math.min(1000 / duration, 0.2); // pause ~1s but max 20% of total

        animation = textRef.closest('.marquee-track')!.animate(
            [
                { transform: 'translateX(0)', offset: 0 },
                { transform: 'translateX(0)', offset: pauseRatio },
                { transform: `translateX(-${scrollDistance}px)`, offset: 1 - pauseRatio },
                { transform: `translateX(-${scrollDistance}px)`, offset: 1 },
            ],
            {
                duration: duration + 4000, // extra time accounts for both pauses
                iterations: Infinity,
                easing: 'linear',
                delay: 1500,
            }
        );
    }

    $effect(() => {
        if (!containerRef || !textRef) return;

        const ro = new ResizeObserver(() => {
            shouldScroll = textRef!.scrollWidth > containerRef!.clientWidth;
            if (!shouldScroll) {
                animation?.cancel();
                animation = null;
            }
        });
        ro.observe(containerRef);
        return () => {
            ro.disconnect();
            animation?.cancel();
        };
    });

    // Start animation once second copy + separator are in the DOM
    $effect(() => {
        if (shouldScroll && separatorRef) {
            // Wait a tick for the DOM to settle
            requestAnimationFrame(() => startAnimation());
        }
    });

    // Reset animation when track changes
    $effect(() => {
        currentTrack.title; // track the dependency
        animation?.cancel();
        animation = null;
        shouldScroll = false;

        // Force re-check since ResizeObserver won't fire if container size didn't change
        requestAnimationFrame(() => {
            if (containerRef && textRef) {
                shouldScroll = textRef.scrollWidth > containerRef.clientWidth;
            }
        });
    });

    let currentTrack = $derived(track ?? {
        id: 0,
        title: "Unknown Track",
        duration: 0,
        artist: { id: 0, name: "Unknown Artist" },
        album: { id: 0, title: "Unknown Album", image: null }
    });
</script>

<div class="flex flex-row w-full gap-3 items-center min-w-0 hover:cursor-pointer" {onclick}>
    <div class="flex-shrink-0">
        {#if currentTrack.album.imageSmall}
            <img src={`${apiHttpService.getBaseUrl()}${currentTrack.album.imageSmall}`} class="rounded-lg" style="height: 3em; width: 3em;" />
        {:else}
            <div class="bg-gray-200 rounded-lg" style="height: 3em; width: 3em;" />
        {/if}
    </div>
    <div class="flex flex-col min-w-0 flex-1">
        <div class="marquee" bind:this={containerRef}>
            <div class="marquee-track">
                <span class="text-sm font-medium whitespace-nowrap text-start" bind:this={textRef}>
                    {currentTrack.title}
                </span>
                {#if shouldScroll}
                    <span class="separator text-sm opacity-30" bind:this={separatorRef}>•</span>
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
</style>