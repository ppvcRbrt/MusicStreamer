// src/lib/bottomSheetState.svelte.ts
import { writable } from 'svelte/store';

export type BottomSheetContent = {
    title: string;
    items: App.Artist | App.Album;
    type: 'artist' | 'album';
} | null;

export const bottomSheetState = writable<BottomSheetContent>(null);

export function openSheet(content: NonNullable<BottomSheetContent>) {
    bottomSheetState.set(content);
}

export function closeSheet() {
    bottomSheetState.set(null);
}
