<script lang="ts">
    import {SearchIcon} from "@lucide/svelte";
    import * as InputGroup from "$lib/components/ui/input-group/index.js";
    let { searchQuery = $bindable() }: { searchQuery: string } = $props();
</script>
<InputGroup.Root class="backdrop-blur-3xl bg-secondary/50">
    <InputGroup.Input bind:value={searchQuery} placeholder="Search..." class="text-center"/>
    <InputGroup.Addon>
        <SearchIcon />
    </InputGroup.Addon>
</InputGroup.Root>
