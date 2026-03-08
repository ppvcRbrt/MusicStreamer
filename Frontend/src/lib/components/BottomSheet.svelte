<!-- src/lib/components/BottomSheet.svelte -->
<script lang="ts">
    import { VerticalSpringSwipe } from '$lib/actions/verticalSpringSwipe.svelte';
    import { swipeable } from '$lib/actions/gestures.svelte';
    import { browser } from '$app/environment';
    import { bottomSheetState, closeSheet, pageState, resetSheet, swipeState } from '../../bottomSheetState.svelte';
    import Tracks from '$lib/components/Tracks.svelte';
    import Playlists from '$lib/components/Playlists.svelte';
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { fly } from 'svelte/transition';
    import { isNativePlatform } from '$lib/utils/platform';
    import { lockScroll, unlockScroll } from "../../bodyOverflowState.svelte";
    import SettingsMenu from "$lib/components/SettingsMenu.svelte";
    import { ChevronLeftIcon } from "@lucide/svelte";
    import ExternalMetadataPage from "$lib/components/ExternalMetadataPage.svelte";
    import { playerState } from "../../musicPlayerState.svelte";

    let windowInnerHeight = $state(browser ? window.innerHeight : 0);
    let sheetHeight = $state(0);

    swipeState.instance = new VerticalSpringSwipe(
        () => windowInnerHeight,
        () => 0,
        windowInnerHeight / 2
    );

    function onHandleClick() {
        if (pageState.showTracks) {
            handleBackTracks();
        }
        else if (pageState.showExternalMetadataMenu) {
            handleBackExternalMetadata();
        }
        else {
            swipeState.instance.onSwipe('down');
        }
    }

    async function getArtistAlbums(artistId: number): Promise<App.Album[]> {
        return await apiHttpService.get<App.Album[]>(`/music/artists/${artistId}/albums`);
    }

    function handlePlaylistItemClick(item: App.Playlist | App.Album) {
        if(item.type === 'album') {
            pageState.selectedItem = item;
            let albumWithoutTracks = { ...item, tracks: [] };
            pageState.selectedItem.tracks.map((t => {
                t.artist = item.artist
                t.album = albumWithoutTracks;
            }));
        }
        pageState.showTracks = true;
    }

    function handleBackTracks() {
        pageState.showTracks = false;
        pageState.selectedItem = null;
    }
    function handleBackExternalMetadata() {
        pageState.showExternalMetadataMenu = false;
        if($bottomSheetState) {
            $bottomSheetState.title = "Settings";
        }
    }
    function handleExternalMetadataClick() {
        pageState.showExternalMetadataMenu = true;
        if($bottomSheetState) {
            $bottomSheetState.title = "Settings  >  External Metadata";
        }
    }
    $effect(() => {
        if ($bottomSheetState && !pageState.wasOpened) {
            swipeState.instance.onSwipe('up');
            pageState.wasOpened = true;
        }
    });

    $effect(() => {
        if (pageState.wasOpened && !swipeState.instance.isUp && swipeState.instance.y.current >= -1) {
            resetSheet();
            closeSheet();
        }
    });

    $effect(() => {
        if (swipeState.instance.isUp) {
            lockScroll('bottom-sheet'); // or 'music-player' in MusicPlayer.svelte
        } else {
            unlockScroll('bottom-sheet'); // or 'music-player' in MusicPlayer.svelte
        }
    });

    let blur = $derived(Math.min(swipeState.instance.progress * 10, 10));
    let elementOpacity = $derived(Math.max(1 - swipeState.instance.progress * 2, 0));

    let headerTitle = $derived(
        pageState.showTracks && pageState.selectedItem
            ? pageState.selectedItem.title ?? ''
            : $bottomSheetState?.title ?? ''
    );
</script>

{#if $bottomSheetState || swipeState.instance.isUp}
    <div
            use:swipeable={{
            axis: "horizontal",
            onDrag: (dy) => swipeState.instance.onDrag(dy),
            onRelease: () => swipeState.instance.onRelease(),
            handle: () => swipeState.instance.handle
        }}
            class="absolute bottom-0 left-0 right-0 z-20"
            style="
            transform: translateY({swipeState.instance.y.current}px);
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
                    bind:this={swipeState.instance.handle}
                    bind:clientHeight={sheetHeight}
                    class="px-4 py-3 flex items-center gap-2 shrink-0"
                    class:safe-area-top={swipeState.instance.isUp && isNativePlatform}
            >
                <button
                        onclick={onHandleClick}
                        class="flex items-center justify-center w-8 h-8 rounded-full hover:bg-white/10 transition-colors shrink-0"
                        aria-label="Go back"
                >
                    <ChevronLeftIcon />
                </button>
                <h2 class="text-lg font-semibold flex-1" onclick={onHandleClick}>
                    {headerTitle}
                </h2>
            </div>

            <!-- Sliding content area -->
            <div class="flex-1 relative overflow-hidden">

                <!-- Albums / Playlists / Queue / Settings View -->
                {#if !pageState.showTracks && !pageState.showExternalMetadataMenu}
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
                            {#key $bottomSheetState.items.tracks}
                                <Tracks tracks={$bottomSheetState.items.tracks} showTrackNumbers={true} height="88%"/>
                            {/key}
                        {:else if $bottomSheetState?.type === 'settings'}
                            <SettingsMenu onExternalMetadataClick={handleExternalMetadataClick}/>
                        {:else if $bottomSheetState?.type === 'queue'}
                            <Tracks tracks={$playerState.playList} showTrackNumbers={false} isQueue={true} imageSize="2.2em" height="88%"/>
                        {/if}
                    </div>
                {/if}

                <!-- Tracks view when clicking from album -->
                {#if pageState.showTracks && pageState.selectedItem}
                    <div
                            class="absolute inset-0 overflow-y-auto"
                            in:fly={{ x: 40, duration: 250 }}
                            out:fly={{ x: 40, duration: 200 }}
                    >
                        <Tracks tracks={pageState.selectedItem.tracks} showTrackNumbers={true} height="88%"/>
                    </div>
                {/if}

                <!-- External Metadata Menu -->
                {#if pageState.showExternalMetadataMenu}
                    <div
                            class="absolute inset-0 overflow-y-auto w-full"
                            in:fly={{ x: 40, duration: 250 }}
                            out:fly={{ x: 40, duration: 200 }}
                    >
                        <ExternalMetadataPage/>
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