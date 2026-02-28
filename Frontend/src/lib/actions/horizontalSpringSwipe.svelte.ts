// lib/utils/horizontalSwipe.svelte.ts
import { Spring } from 'svelte/motion';

export class HorizontalSpringSwipe {
    x = new Spring(0, { stiffness: 0.2, damping: 0.8 });
    isLeft = $state(false);
    isRight = $state(false);
    handle = $state<HTMLElement | null>(null);

    private width: () => number;
    private panelWidth: () => number;

    constructor(width: () => number, panelWidth: () => number) {
        this.width = width;
        this.panelWidth = panelWidth;
    }

    get leftPos() {
        return -this.width() + this.panelWidth();
    }

    get rightPos() {
        return this.width() - this.panelWidth();
    }

    get progress() {
        return Math.min(Math.abs(this.x.current) / (this.width() - this.panelWidth()), 1);
    }

    onDrag(dx: number) {
        if (this.isLeft) {
            if (dx < 0) {
                this.x.set(this.leftPos + dx / 15, { instant: true }); // resist further left
            } else {
                this.x.set(Math.min(this.leftPos + dx, 0), { instant: true }); // clamp to center
            }
        } else if (this.isRight) {
            if (dx > 0) {
                this.x.set(this.rightPos + dx / 15, { instant: true }); // resist further right
            } else {
                this.x.set(Math.max(this.rightPos + dx, 0), { instant: true }); // clamp to center
            }
        } else {
            if (dx < 0) {
                this.x.set(Math.max(dx, this.leftPos), { instant: true }); // clamp to left
            } else {
                this.x.set(Math.min(dx, this.rightPos), { instant: true }); // clamp to right
            }
        }
    }

    onSwipe(dir: 'left' | 'right') {
        if (dir === 'left' && !this.isLeft) {
            this.isLeft = true;
            this.isRight = false;
            this.x.set(this.leftPos);
        } else if (dir === 'right' && !this.isRight) {
            this.isRight = true;
            this.isLeft = false;
            this.x.set(this.rightPos);
        }
    }

    onRelease() {
        const width = this.width();

        if (this.isLeft) {
            if (this.x.current > this.leftPos + width * 0.25) {
                this.isLeft = false;
                this.x.set(0); // snap back to center
            } else {
                this.x.set(this.leftPos); // snap back to left
            }
        } else if (this.isRight) {
            if (this.x.current < this.rightPos - width * 0.25) {
                this.isRight = false;
                this.x.set(0); // snap back to center
            } else {
                this.x.set(this.rightPos); // snap back to right
            }
        } else {
            if (this.x.current < -(width * 0.25)) {
                this.onSwipe('left');
            } else if (this.x.current > width * 0.25) {
                this.onSwipe('right');
            } else {
                this.x.set(0); // snap back to center
            }
        }
    }
}