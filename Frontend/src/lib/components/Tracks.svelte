<script lang="ts">
    import {ScrollArea} from "$lib/components/ui/scroll-area";
    import {Button} from "$lib/components/ui/button";
    import {ListMinusIcon, ListPlusIcon} from "@lucide/svelte";
    import {playerState} from "../../musicPlayerState.svelte";
    import {apiHttpService} from "$lib/services/apiHttpService";
    import {HorizontalSpringSwipe} from "$lib/actions/horizontalSpringSwipe.svelte";
    import {swipeable} from '$lib/actions/gestures.svelte';
    import {slide} from "svelte/transition";
    import {hapticHeavy, hapticLight} from "$lib/utils/haptics";
    import {logEvent} from "$lib/services/listeningEventService.ts";
    import {ListeningEventType} from "$lib/utils/enums.ts";
    import {getCurrentContext} from "../../bottomSheetState.svelte.ts";
    import { DragReorder } from "$lib/actions/dragReorder.svelte";
    import {GripVerticalIcon} from "@lucide/svelte";
    import { lockScroll, unlockScroll } from "../../bodyOverflowState.svelte"; // optional but recommended

    let { tracks, imageSize, height, showTrackNumbers=false, isQueue=false }:
        { tracks: App.Track[], imageSize: string, height: string, showTrackNumbers?:boolean } = $props();

    let tracksOrdered = $state(tracks.sort((a, b) => a.trackNumber - b.trackNumber));
    let currentlyPlayingTrackId = $derived($playerState.playList[$playerState.trackIndex]?.id ?? -1);

    let itemHeights = $state<number[]>([]);
    let isDragReorder = $state(false);

    // CHANGED: id-keyed handle registry (stable across reorder)
    let handleElsById = $state<Record<number, HTMLElement | null>>({});

    const reorder = new DragReorder(() => itemHeights[0] ?? 64);

    function onTrackClick(trackIndex: number) {
        if (tracksOrdered[trackIndex].id !== currentlyPlayingTrackId) {
            $playerState.playList = tracksOrdered;
            $playerState.trackIndex = trackIndex;
            $playerState.currentTime = 0;
            $playerState.isPlaying = true;
            hapticLight();
        } else {
            hapticHeavy();
        }
    }

    function handleRemoveAddFromQueue(track: App.Track) {
        if (isTrackInQueue(track)) {
            logEvent(track.id, 0, 0, ListeningEventType.QueueRemove, getCurrentContext());
            if (track.id === currentlyPlayingTrackId) {
                $playerState.isPlaying = false;
                $playerState.currentTime = 0;
            }

            const removedIndex = $playerState.playList.findIndex(t => t.id === track.id);
            $playerState.playList = $playerState.playList.filter(t => t.id !== track.id);

            if (removedIndex < $playerState.trackIndex) {
                $playerState.trackIndex = $playerState.trackIndex - 1;
            }

            if ($playerState.trackIndex >= $playerState.playList.length) {
                $playerState.trackIndex = Math.max(0, $playerState.playList.length - 1);
            }

            if (isQueue) {
                tracksOrdered = $playerState.playList;
            }
        } else {
            const trackToAdd = { ...track, trackNumber: $playerState.playList.length + 1 };
            $playerState.playList = [...$playerState.playList, trackToAdd];
            logEvent(track.id, 0, 0, ListeningEventType.QueueRemove, getCurrentContext());
        }
    }

    function isTrackInQueue(track: App.Track) {
        return $playerState.playList.some(t => t.id === track.id);
    }

    function maybeSlide(node: Element, params: { duration: number }) {
        if (isDragReorder) return {};
        return slide(node, params);
    }

    // NEW: resolve drag start by id -> current index
    function startReorderByTrackId(trackId: number) {
        const idx = tracksOrdered.findIndex((t) => t.id === trackId);
        if (idx !== -1) reorder.onDragStart(idx);
    }

    // NEW: lookup used by swipe handle callback
    function getHandleEl(trackId: number): HTMLElement | null {
        return handleElsById[trackId] ?? null;
    }

    // NEW: stable handle registration action
    function registerHandle(node: HTMLElement, trackId: number) {
        handleElsById[trackId] = node;
        return {
            update(newTrackId: number) {
                if (newTrackId === trackId) return;
                if (handleElsById[trackId] === node) delete handleElsById[trackId];
                trackId = newTrackId;
                handleElsById[trackId] = node;
            },
            destroy() {
                if (handleElsById[trackId] === node) delete handleElsById[trackId];
            }
        };
    }

    // OPTIONAL: lock page scroll during active reorder only
    $effect(() => {
        if (reorder.isDragging) {
            lockScroll("tracks-reorder");
        } else {
            unlockScroll("tracks-reorder");
        }
        return () => unlockScroll("tracks-reorder");
    });
</script>

{#snippet Track(track: App.Track, trackIndex: number)}
    {@const swipe = new HorizontalSpringSwipe(() => 100, () => 60)}
    {@const isDragged = reorder.dragIndex === trackIndex}
    {@const displacement = (() => {
        if (!reorder.isDragging || isDragged) return 0;
        const from = reorder.dragIndex!;
        const to = reorder.targetIndex!;
        const h = itemHeights[0] ?? 65;
        if (from < to && trackIndex > from && trackIndex <= to) return -h;
        if (from > to && trackIndex < from && trackIndex >= to) return h;
        return 0;
    })()}

    <div class="border-b {trackIndex === 0 ? 'border-t' : ''}">
        <div
                class="relative overflow-hidden transition-shadow"
                class:shadow-xl={isDragged}
                class:bg-secondary={isDragged}
                class:z-10={isDragged}
                style="transform: translateY({isDragged ? reorder.y.current : displacement}px);
                   transition: transform {reorder.isDragging ? (isDragged ? '0ms' : '150ms') : '0ms'} ease;"
                bind:clientHeight={itemHeights[trackIndex]}
                use:swipeable={{
                axis: "horizontal",
                onDrag: (_dy, dx) => {
                    if (!reorder.isDragging) {
                        swipe.onDrag(swipe.isRight ? Math.min(dx ?? 0, 0) : Math.max(dx ?? 0, 0));
                    }
                },
                onRelease: () => {
                    if (!reorder.isDragging) swipe.onRelease();
                }
            }}
        >
            <div
                    class="flex items-center gap-0.5"
                    style="transform: translateX({swipe.x.current}px)"
                    use:swipeable={{
                    axis: "vertical",
                    handle: () => (isQueue ? getHandleEl(track.id) : null), // CHANGED
                    onDrag: (dy) => {
                        if (reorder.isDragging) reorder.onDrag(dy);
                    },
                    onRelease: () => {
                        if (reorder.isDragging) {
                            isDragReorder = true;
                            tracksOrdered = reorder.onRelease(tracksOrdered)
                                .map((t, i) => ({ ...t, trackNumber: i + 1 }));

                            if (isQueue) {
                                const currentTrackId = $playerState.playList[$playerState.trackIndex]?.id;
                                const newTrackIndex = tracksOrdered.findIndex(t => t.id === currentTrackId);
                                $playerState.playList = [...tracksOrdered];
                                if (newTrackIndex !== -1) $playerState.trackIndex = newTrackIndex;
                            }

                            setTimeout(() => isDragReorder = false, 0);
                        }
                    }
                }}
            >
                <Button
                        variant="ghost"
                        size="icon"
                        class="flex-shrink-0 -ml-9 flex items-center justify-center shadow-2xl"
                        onclick={() => {
                        swipe.isRight = false;
                        swipe.x.set(0);
                        handleRemoveAddFromQueue(track);
                    }}
                >
                    {#if isTrackInQueue(track)}
                        <ListMinusIcon class="text-red-200"/>
                    {:else}
                        <ListPlusIcon class="text-green-200"/>
                    {/if}
                </Button>

                <div class="w-full min-w-0">
                    <Button
                            variant="ghost"
                            class="flex flex-row items-center gap-2 py-6 w-full rounded-none min-w-0
                               {track.id === currentlyPlayingTrackId ? 'bg-primary/10 hover:bg-primary/15 border-l-2 border-l-primary py-6' : ''}"
                            onclick={() => onTrackClick(trackIndex)}
                    >
                        {#if showTrackNumbers}
                            <span class="w-6 text-left opacity-50">{track.trackNumber}</span>
                        {:else}
                            {#if track.album.imageSmall}
                                <img src={`${apiHttpService.getBaseUrl()}${track.album.imageSmall}`} alt={track.title} class="rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};"/>
                            {:else}
                                <div class="bg-gray-200 rounded-lg flex-shrink-0" style="height: {imageSize}; width: {imageSize};" />
                            {/if}
                        {/if}
                        <div class="flex flex-col items-start justify-start flex-1 min-w-0">
                            <p class="text-sm font-medium truncate w-full text-start">{track.title}</p>
                            <div class="flex flex-row gap-1 text-xs italic opacity-50 min-w-0 w-full text-start">
                                <span class="flex-shrink-0">{track.artist.name}</span>
                                <span class="flex-shrink-0">•</span>
                                <span class="min-w-0 truncate">{track.album.title}</span>
                            </div>
                        </div>
                    </Button>
                </div>

                {#if isQueue}
                    <div
                            use:registerHandle={track.id}
                    class="cursor-grab p-3 text-muted-foreground"
                    ontouchstart={() => startReorderByTrackId(track.id)}
                    >
                    <GripVerticalIcon size={16} />
                    </div>
                {/if}
            </div>
        </div>
    </div>
{/snippet}


<ScrollArea
        style="height: {height};"
        class="flex w-full p-4"
        orientation="vertical">
        <div class="flex flex-col w-full justify-items-start">
            {#each tracksOrdered as track, index (track.id)}
                <div transition:maybeSlide={{ duration: 300 }}>
                    {@render Track(track, index)}
                </div>
            {/each}
        </div>
</ScrollArea>

