// src/lib/bottomSheetState.svelte.ts
import { writable } from 'svelte/store';
import type {VerticalSpringSwipe} from "$lib/actions/verticalSpringSwipe.svelte";
import {ContextType} from "$lib/utils/enums";

export type BottomSheetContent = {
    title: string;
    items?: App.Artist | App.Album | App.Playlist;
    type: 'artist' | 'album' | 'settings' | 'queue' ;
} | null;

export interface PageState {
    wasOpened: boolean;
    showTracks: boolean;
    selectedItem: App.Playlist | App.Album | null;
    showExternalMetadataMenu: boolean;
}
export function resetSheet() {
    pageState.wasOpened = false;
    pageState.showTracks = false;
    pageState.selectedItem = null;
    pageState.showExternalMetadataMenu = false;
}

export let pageState = $state<PageState>({
    wasOpened: false,
    showTracks: false,
    selectedItem: null,
    showExternalMetadataMenu: false
});

export let swipeState = $state<{ instance: VerticalSpringSwipe | null }>({ instance: null });

export const bottomSheetState = writable<BottomSheetContent>(null);

export function openSheet(content: NonNullable<BottomSheetContent>) {
    bottomSheetState.set(content);
}

export function closeSheet() {
    bottomSheetState.set(null);
}

export function getCurrentContext() {
    let currentContext: ContextType = ContextType.Queue;
    let currentSheet = ""
    bottomSheetState.subscribe(value => currentSheet = value?.type ?? "")

    switch (currentSheet) {
        case "queue":
            currentContext = ContextType.Queue;
            break;
        case "playlist":
            currentContext = ContextType.Playlist;
            break;
        case "album":
            currentContext = ContextType.Album;
            break;
        default:
            currentContext = ContextType.Queue;
            break;
    }
    return currentContext;
}
