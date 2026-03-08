export function swipeable(node: HTMLElement, callbacks: {
    onSwipe?: (dir: 'up' | 'down' | 'left' | 'right') => void;
    onDrag?: (dy: number, dx?: number) => void;
    onRelease?: () => void;
    threshold?: number;
    handle?: () => HTMLElement;
    axis?: 'vertical' | 'horizontal' | 'both';
}) {
    let startX = 0;
    let startY = 0;
    let dragging = false;
    let currentCallbacks = callbacks;

    function getAxis() {
        return currentCallbacks.axis ?? 'vertical';
    }

    function onTouchStart(e: TouchEvent) {
        const handle = currentCallbacks.handle?.();
        if (handle && !handle.contains(e.target as Node)) return;
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
        dragging = true;
    }

    function onTouchMove(e: TouchEvent) {
        if (!dragging) return;
        e.preventDefault();
        const dx = e.touches[0].clientX - startX;
        const dy = e.touches[0].clientY - startY;
        currentCallbacks.onDrag?.(dy, dx);
    }

    function onTouchEnd(e: TouchEvent) {
        if (!dragging) return;
        dragging = false;
        const dx = e.changedTouches[0].clientX - startX;
        const dy = e.changedTouches[0].clientY - startY;
        currentCallbacks.onRelease?.();

        const axis = getAxis();
        const threshold = currentCallbacks.threshold ?? 50;

        if (axis === 'horizontal') {
            if (Math.abs(dx) > threshold) {
                currentCallbacks.onSwipe?.(dx > 0 ? 'right' : 'left');
            }
        } else if (axis === 'vertical') {
            if (Math.abs(dy) > threshold) {
                currentCallbacks.onSwipe?.(dy > 0 ? 'down' : 'up');
            }
        } else {
            // 'both' — dominant axis wins
            if (Math.abs(dx) > Math.abs(dy) && Math.abs(dx) > threshold) {
                currentCallbacks.onSwipe?.(dx > 0 ? 'right' : 'left');
            } else if (Math.abs(dy) > threshold) {
                currentCallbacks.onSwipe?.(dy > 0 ? 'down' : 'up');
            }
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