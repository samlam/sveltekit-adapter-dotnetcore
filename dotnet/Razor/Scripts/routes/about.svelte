<script context="module" lang="ts">
	import { browser, dev } from '$app/env';
	import type {JokeApiResponse} from '../lib/jokeService'
	import JokeCard from '../lib/components/jokeCard.svelte'

	export const hydrate = dev;
	export const prerender = true;

	const pageName = 'About'
</script>

<script lang="ts">
	export let jokeResponse: JokeApiResponse
</script>

<svelte:head>
	<title>Sveltekit Page - {pageName}</title>
</svelte:head>

<section> 
	<h1>Sveltekit + dotnet - {pageName} page</h1>
	<br/>
	<JokeCard {jokeResponse}></JokeCard>
	<br/>
	<a href="./" rel="external">back to home</a>
</section>

<style>
	section {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		flex: 1;
		height: 90vh;
	}
</style>
