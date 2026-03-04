<!-- src/lib/components/BottomSheet.svelte -->
<script lang="ts">
    import { VerticalSpringSwipe } from '$lib/actions/verticalSpringSwipe.svelte';
    import { swipeable } from '$lib/actions/gestures.svelte';
    import { browser } from '$app/environment';
    import { bottomSheetState, closeSheet } from '../../bottomSheetState.svelte';
    import Tracks from '$lib/components/Tracks.svelte';
    import Playlists from '$lib/components/Playlists.svelte';
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { fly } from 'svelte/transition';
    import { isNativePlatform } from '$lib/utils/platform';
    import {lockScroll, unlockScroll} from "../../bodyOverflowState.svelte";

    let windowInnerHeight = $state(browser ? window.innerHeight : 0);
    let sheetHeight = $state(0);
    let wasOpened = $state(false);
    let selectedItem = $state<App.Playlist | App.Album | null>(null);
    let showTracks = $state(false);

    const swipe = new VerticalSpringSwipe(
        () => windowInnerHeight,
        () => 0,
        windowInnerHeight / 2
    );

    function onHandleClick() {
        if (showTracks) {
            handleBack();
        } else {
            swipe.onSwipe('down');
        }
    }

    async function getArtistAlbums(artistId: number): Promise<App.Album[]> {
        return await apiHttpService.get<App.Album[]>(`/music/artists/${artistId}/albums`);
    }

    function handlePlaylistItemClick(item: App.Playlist | App.Album) {
        if(item.type === 'album') {
            selectedItem = item;
            let albumWithoutTracks = { ...item, tracks: [] };
            selectedItem.tracks.map((t => {
                t.artist = item.artist
                t.album = albumWithoutTracks;
            }));
        }
        showTracks = true;
    }

    function handleBack() {
        showTracks = false;
        selectedItem = null;
    }

    $effect(() => {
        if ($bottomSheetState && !wasOpened) {
            swipe.onSwipe('up');
            wasOpened = true;
        }
    });

    $effect(() => {
        if (wasOpened && !swipe.isUp && swipe.y.current >= -1) {
            console.log("Closing sheet");
            wasOpened = false;
            showTracks = false;
            selectedItem = null;
            closeSheet();
        }
    });

    $effect(() => {
        if (swipe.isUp) {
            lockScroll('bottom-sheet'); // or 'music-player' in MusicPlayer.svelte
        } else {
            unlockScroll('bottom-sheet'); // or 'music-player' in MusicPlayer.svelte
        }
    });

    let blur = $derived(Math.min(swipe.progress * 10, 10));
    let elementOpacity = $derived(Math.max(1 - swipe.progress * 2, 0));

    let headerTitle = $derived(
        showTracks && selectedItem
            ? selectedItem.title ?? ''
            : $bottomSheetState?.title ?? ''
    );
</script>

{#if $bottomSheetState || swipe.isUp}
    <div
            use:swipeable={{
            onDrag: (dy) => swipe.onDrag(dy),
            onRelease: () => swipe.onRelease(),
            handle: () => swipe.handle
        }}
            class="absolute bottom-0 left-0 right-0 z-20"
            style="
            transform: translateY({swipe.y.current}px);
            margin-bottom: -{windowInnerHeight}px;
            position: relative;
        "
    >
        <!-- Pull handle -->
        <div class="flex h-1.5 justify-center hover:cursor-pointer">
            <div class="w-[42.5%] border-b"></div>
            <div class="w-[15%] border-t rounded-t-xl backdrop-blur-xl bg-secondary/70"></div>
            <div class="w-[42.5%] border-b"></div>
        </div>

        <div
                class="backdrop-blur-xl bg-secondary/70 flex flex-col overflow-hidden"
                style="height: {windowInnerHeight}px;"
        >
            <!-- Header / drag handle -->
            <div
                    bind:this={swipe.handle}
                    bind:clientHeight={sheetHeight}
                    class="px-4 py-3 flex items-center gap-2 shrink-0"
                    class:safe-area-top={swipe.isUp && isNativePlatform}
            >
                <button
                        onclick={showTracks ? handleBack : () => swipe.onSwipe('down')}
                        class="flex items-center justify-center w-8 h-8 rounded-full hover:bg-white/10 transition-colors shrink-0"
                        aria-label="Go back"
                >
                    <!-- Left chevron icon -->
                    <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <polyline points="15 18 9 12 15 6" />
                    </svg>
                </button>
                <h2 class="text-lg font-semibold flex-1" onclick={onHandleClick}>
                    {headerTitle}
                </h2>
            </div>

            <!-- Sliding content area -->
            <div class="flex-1 relative overflow-hidden">

                <!-- Albums / Playlists view -->
                {#if !showTracks}
                    <div
                            class="absolute inset-0 overflow-y-auto"
                            in:fly={{ x: -40, duration: 250 }}
                            out:fly={{ x: -40, duration: 200 }}
                    >
                        {#if $bottomSheetState?.type === 'artist'}
                            {#await getArtistAlbums(($bottomSheetState.items).id) then albums}
                                <Playlists
                                        playlists={albums}
                                        imageSize="4em"
                                        Class="mt-5"
                                        onItemClick={handlePlaylistItemClick}
                                />
                            {:catch error}
                                <p class="text-sm text-red-500 px-4">Failed to load albums.</p>
                            {/await}
                        {:else if $bottomSheetState?.type === 'album'}
                            <Tracks tracks={$bottomSheetState.items.tracks} imageSize="2.2em" height="88%"/>
                        {/if}
                    </div>
                {/if}

                <!-- Tracks view -->
                {#if showTracks && selectedItem}
                    <div
                            class="absolute inset-0 overflow-y-auto"
                            in:fly={{ x: 40, duration: 250 }}
                            out:fly={{ x: 40, duration: 200 }}
                    >
                        <Tracks tracks={selectedItem.tracks} imageSize="2.2em" height="88%"/>
                    </div>
                {/if}

            </div>
        </div>
    </div>
{/if}

<style>
    .safe-area-top {
        padding-top: env(safe-area-inset-top);
    }
</style>