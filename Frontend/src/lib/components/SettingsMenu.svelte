<script lang="ts">
    import { Button } from "$lib/components/ui/button";
    import { Spinner } from "$lib/components/ui/spinner";
    import { apiHttpService } from "$lib/services/apiHttpService";
    import * as Dialog from "$lib/components/ui/dialog/index.js";

    let isLoadingLocalLibrary = $state(false);
    let isDialogOpen = $state(false);
    let dialogTitle = $state("");
    let dialogDescription = $state("");

    async function handleLoadLocalLibrary() {
        isLoadingLocalLibrary = true;
        try {
            let result = await apiHttpService.get<App.TrackStoreResult>("/file/loadLocalTracks")
            dialogTitle = result.message;
            dialogDescription = `Loaded ${result.tracksAdded} tracks.\n Loaded ${result.albumsAdded} albums.\n Loaded ${result.artistsAdded} artists.`;
            isDialogOpen = true;
        }
        catch(e) {
            console.error("Error loading local library", e);
        }
        finally {
            isLoadingLocalLibrary = false;
        }
    }
</script>

<div class="flex flex-col gap-2">
    <Button variant="outline" onclick={handleLoadLocalLibrary} disabled={isLoadingLocalLibrary}>
        {#if isLoadingLocalLibrary}
            <Spinner />
            Scanning local library...
        {:else}
            Load local library
        {/if}
    </Button>
    <!-- This will require a signalr connection to get progress updates, so we'll implement it later -->
    <Button variant="outline">
        Find library artists on discogs
    </Button>
</div>

<Dialog.Root open={isDialogOpen}>
    <Dialog.Content>
        <Dialog.Header>
            <Dialog.Title>{dialogTitle}</Dialog.Title>
            <Dialog.Description class="whitespace-pre-line">
                {dialogDescription}
            </Dialog.Description>
        </Dialog.Header>
    </Dialog.Content>
</Dialog.Root>
