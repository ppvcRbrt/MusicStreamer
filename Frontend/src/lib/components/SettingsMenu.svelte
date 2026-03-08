<script lang="ts">
    import { Button } from "$lib/components/ui/button";
    import { Spinner } from "$lib/components/ui/spinner";
    import { apiHttpService } from "$lib/services/apiHttpService";
    import { ChevronRightIcon } from "@lucide/svelte";
    import * as Dialog from "$lib/components/ui/dialog/index.js";
    import * as Select from "$lib/components/ui/select/index.js";
    import {userSettings} from "../../settingsState.svelte";
    import { Checkbox } from "$lib/components/ui/checkbox/index.js";
    import { Label } from "$lib/components/ui/label/index.js";
    import {getPreferredAudioQuality} from "$lib/utils/network";
    import {onMount} from "svelte";
    interface SyncStatus {
        transcoding: App.TranscodingStatus;
    }

    let { onExternalMetadataClick }: { onExternalMetadataClick?: () => void } = $props();
    let isLoadingLocalLibrary = $state(false);
    let isDialogOpen = $state(false);
    let dialogTitle = $state("");
    let dialogDescription = $state("");
    let preferredAudioFormats = [
        { label: "Opus", value: "opus" },
        { label: "FLAC", value: "flac" },
    ];
    let syncStatus = $state<SyncStatus | null>(null);

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

    async function handleTriggerTranscoding() {
        try {
            await apiHttpService.get("/file/triggerTranscoding");
            syncStatus = await apiHttpService.get<SyncStatus>('/externalMetadata/sync/status');
        }
        catch(e) {
            console.error("Error triggering transcoding", e);
        }
    }
    function handleAutoAudioChanged(enableAutoAudio: boolean) {
        if(enableAutoAudio) {
            $userSettings.preferredAudioFormat = getPreferredAudioQuality();
        }
    }
    $effect(() => {
        const interval = setInterval(async () => {
            syncStatus = await apiHttpService.get<SyncStatus>('/externalMetadata/sync/status');
            console.log("syncStatus", syncStatus);
        }, 2000);

        return () => clearInterval(interval);
    });
    onMount(async () => {
        syncStatus = await apiHttpService.get<SyncStatus>('/externalMetadata/sync/status');
    });

</script>

<div class="flex flex-col gap-2 mx-3">
    <Button variant="outline" onclick={handleLoadLocalLibrary} disabled={isLoadingLocalLibrary}>
        {#if isLoadingLocalLibrary}
            <Spinner />
            Scanning local library...
        {:else}
            Load Local Library
        {/if}
    </Button>
    <Button class="relative overflow-hidden" variant="outline" onclick={handleTriggerTranscoding} disabled={syncStatus?.transcoding?.state === "Running"}>
        Transcode Tracks to Opus
            <div class="w-full absolute bottom-0 h-0.5 rounded-full
                {syncStatus?.transcoding.state === 'Running' ? 'bg-amber-500 animate-pulse' : ''}
                {syncStatus?.transcoding.state === 'Idle' ? 'bg-emerald-500 animate-pulse' : ''}"
            />
    </Button>
    <Button class="relative flex w-full" variant="outline" onclick={onExternalMetadataClick}>
        <span class="absolute left-1/2 -translate-x-1/2">External Metadata</span>
        <ChevronRightIcon class="ml-auto" />
    </Button>
    <div class="flex flex-row justify-center gap-2 items-center text-center mt-2">
        <Checkbox id="autoAudioPref" bind:checked={$userSettings.autoPreferredAudioFormat} onCheckedChange={handleAutoAudioChanged}/>
        <Label for="autoAudioPref">Enable Auto Audio Format</Label>
    </div>
    <Select.Root type="single" bind:value={$userSettings.preferredAudioFormat} disabled={$userSettings.autoPreferredAudioFormat}>
        <Select.Trigger class="flex w-full justify-center">
            <span>Preferred Audio Format: {$userSettings.preferredAudioFormat}</span>
        </Select.Trigger>
        <Select.Content class="w-full">
            <Select.Group>
                <Select.Label>Preferred Audio Format</Select.Label>
                {#each preferredAudioFormats as format (format.value)}
                    <Select.Item
                            value={format.value}
                            label={format.label}
                    >
                        {format.label}
                    </Select.Item>
                {/each}
            </Select.Group>
        </Select.Content>
    </Select.Root>
    <Button variant="outline" onclick={() => window.location.reload()}>
        Refresh
    </Button>

</div>

<Dialog.Root bind:open={isDialogOpen}>
    <Dialog.Content>
        <Dialog.Header>
            <Dialog.Title>{dialogTitle}</Dialog.Title>
            <Dialog.Description class="whitespace-pre-line">
                {dialogDescription}
            </Dialog.Description>
        </Dialog.Header>
    </Dialog.Content>
</Dialog.Root>
