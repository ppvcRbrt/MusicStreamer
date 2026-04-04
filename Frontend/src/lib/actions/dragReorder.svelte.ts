// src/lib/actions/dragReorder.svelte.ts
import { Spring } from 'svelte/motion';

export class DragReorder<T> {
    /** Spring-animated Y translation for the dragged item */
    y = new Spring(0, { stiffness: 0.3, damping: 0.8 });

    dragIndex = $state<number | null>(null);
    targetIndex = $state<number | null>(null);

    private itemHeight: () => number;

    constructor(itemHeight: () => number) {
        this.itemHeight = itemHeight;
    }

    get isDragging() {
        return this.dragIndex !== null;
    }

    onDragStart(index: number) {
        this.dragIndex = index;
        this.targetIndex = index;
        this.y.set(0, { instant: true });
    }

    onDrag(dy: number) {
        if (this.dragIndex === null) return;
        this.y.set(dy, { instant: true });

        const h = this.itemHeight();
        const steps = Math.round(dy / h);
        this.targetIndex = Math.max(
            0,
            this.dragIndex + steps
            // upper bound is clamped in the component
        );
    }

    onRelease<T>(list: T[]): T[] {
        if (this.dragIndex === null || this.targetIndex === null) return list;

        const from = this.dragIndex;
        const to = Math.min(this.targetIndex, list.length - 1);

        const reordered = [...list];
        const [item] = reordered.splice(from, 1);
        reordered.splice(to, 0, item);

        this.dragIndex = null;
        this.targetIndex = null;
        this.y.set(0);

        return reordered;
    }
}