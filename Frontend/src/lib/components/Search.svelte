<script lang="ts">
    import {SearchIcon} from "@lucide/svelte";
    import * as InputGroup from "$lib/components/ui/input-group/index.js";
    let { searchQuery = $bindable(), Class="" }:
        { searchQuery: string, Class: string } = $props();
</script>
<InputGroup.Root class="{Class}">
    <InputGroup.Input bind:value={searchQuery} placeholder="Search..." class="text-center"/>
    <InputGroup.Addon>
        <SearchIcon />
    </InputGroup.Addon>
</InputGroup.Root>
