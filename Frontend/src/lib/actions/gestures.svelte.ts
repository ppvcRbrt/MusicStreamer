export function swipeable(node: HTMLElement, callbacks: {
    onSwipe?: (dir: 'up' | 'down') => void;
    onDrag?: (dy: number) => void;
    onRelease?: () => void;
    threshold?: number;
    handle?: () => HTMLElement;
}) {
    let startY = 0;
    let dragging = false;
    let currentCallbacks = callbacks;

    function onTouchStart(e: TouchEvent) {
        const handle = currentCallbacks.handle?.();
        if (handle && !handle.contains(e.target as Node)) return; // ignore if not in handle
        startY = e.touches[0].clientY;
        dragging = true;
    }

    function onTouchMove(e: TouchEvent) {
        if (!dragging) return;
        e.preventDefault();
        currentCallbacks.onDrag?.(e.touches[0].clientY - startY);
    }

    function onTouchEnd(e: TouchEvent) {
        if (!dragging) return;
        dragging = false;
        const dy = e.changedTouches[0].clientY - startY;
        currentCallbacks.onRelease?.();
        if (Math.abs(dy) > (currentCallbacks.threshold ?? 50)) {
            currentCallbacks.onSwipe?.(dy > 0 ? 'down' : 'up');
        }
    }

    function onTouchCancel() {
        if (!dragging) return;
        dragging = false;
        currentCallbacks.onRelease?.();
    }

    node.addEventListener('touchstart', onTouchStart, { passive: true });
    node.addEventListener('touchmove', onTouchMove, { passive: false });
    node.addEventListener('touchend', onTouchEnd, { passive: true });
    node.addEventListener('touchcancel', onTouchCancel, { passive: true });
    node.style.touchAction = 'none';

    return {
        update(newCallbacks: typeof callbacks) {
            currentCallbacks = newCallbacks;
        },
        destroy() {
            node.removeEventListener('touchstart', onTouchStart);
            node.removeEventListener('touchmove', onTouchMove);
            node.removeEventListener('touchend', onTouchEnd);
            node.removeEventListener('touchcancel', onTouchCancel);
        }
    };
}