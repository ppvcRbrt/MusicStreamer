<script lang="ts">
    import { errorState, clearServerError } from "../../errorState.svelte";
    import { RefreshCwIcon } from '@lucide/svelte';
    import { Button } from '$lib/components/ui/button';

    function refresh() {
        clearServerError();
        window.location.reload();
    }
</script>

{#if errorState.hasError}
    <div class="fixed inset-0 z-50 flex flex-col items-center justify-center bg-background/90 backdrop-blur-sm gap-4">
        <p class="text-destructive text-lg font-medium">{errorState.message}</p>
        <Button onclick={refresh} variant="outline" class="gap-2">
            <RefreshCwIcon size={16} />
            Retry
        </Button>
    </div>
{/if}