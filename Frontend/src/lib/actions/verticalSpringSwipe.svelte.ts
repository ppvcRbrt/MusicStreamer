// lib/utils/verticalSpringSwipe.svelte.ts
import { Spring } from 'svelte/motion';

export class VerticalSpringSwipe {
    y = new Spring(0, { stiffness: 0.2, damping: 0.8 });
    isUp = $state(false);
    handle = $state<HTMLElement | null>(null);

    private height: () => number;
    private panelHeight: () => number;
    private swipeDownThreshold?: number;

    constructor(height: () => number, panelHeight: () => number, swipeDownThreshold?: number) {
        this.height = height;
        this.panelHeight = panelHeight;
        this.swipeDownThreshold = swipeDownThreshold;
    }

    get top() {
        return -this.height() + this.panelHeight();
    }

    get progress() {
        return Math.min(Math.abs(this.y.current) / (this.height() - this.panelHeight()), 1);
    }

    onDrag(dy: number) {
        if (this.isUp) {
            if (dy < 0) {
                this.y.set(this.top + dy / 15, { instant: true });
            } else {
                this.y.set(Math.min(this.top + dy, 0), { instant: true });
            }
        } else {
            if (dy > 0) {
                this.y.set(dy / 15, { instant: true });
            } else {
                this.y.set(Math.max(dy, this.top), { instant: true });
            }
        }
    }

    onSwipe(dir: 'up' | 'down') {
        if (dir === 'up' && !this.isUp) {
            this.isUp = true;
            this.y.set(this.top);
        }
        else if (dir === 'down' && this.isUp) {
            console.log(`Swiping down: current y=${this.y.current}, top=${this.top}`);
            this.isUp = false;
            this.y.set(0);
        }
    }

    onRelease() {
        const height = this.height();
        if (!this.isUp && this.y.current < -(height * 0.25)) {
            this.onSwipe('up');
        }
        else if (this.isUp && this.swipeDownThreshold !== undefined && this.y.current > this.top + this.swipeDownThreshold) {
            this.onSwipe('down');
        }
        else if (this.isUp && this.y.current > -(height * 0.75)) {
            this.onSwipe('down');
        }
        else if (this.isUp) {
            this.y.set(this.top);
        }
        else {
            this.y.set(0);
        }
    }
}