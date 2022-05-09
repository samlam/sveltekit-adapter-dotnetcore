<script context="module" lang="ts">
	import { browser, dev } from '$app/env';
	export const hydrate = dev;
	export const prerender = true;

	const pageName = 'About'
</script>

<svelte:head>
	<title>Sveltekit Page - {pageName}</title>
</svelte:head>


<section> 
	<h1>Sveltekit + dotnet - {pageName} page</h1>
</section>

<style>
	section {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		flex: 1;
	}
</style>
