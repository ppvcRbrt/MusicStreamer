// src/lib/utils/bodyOverflow.ts
import { browser } from '$app/environment';

let locks = $state(new Set<string>());

export function lockScroll(id: string) {
    locks.add(id);
    if (browser) document.body.style.overflow = 'hidden';
}

export function unlockScroll(id: string) {
    locks.delete(id);
    if (browser && locks.size === 0) document.body.style.overflow = '';
}
