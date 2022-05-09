<script context="module" lang="ts">
	export const prerender = true;
</script>

<svelte:head>
	<title>Sveltekit Page</title>
</svelte:head>


<section> 
	<h1>this component is rendered by Sveltekit + dotnet</h1>
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
