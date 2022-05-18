<script context="module" lang="ts">
	import type {JokeApiResponse} from '../../lib/jokeService'
	import JokeCard from '../../lib/components/jokeCard.svelte'
</script>

<script lang="ts">
	export let jokeResponse: JokeApiResponse
    export let id: string
    const pageName = `Joke ${id}`
</script>

<svelte:head>
	<title>Sveltekit Page - {pageName}</title>
</svelte:head>

<section> 
	<br/>
	<JokeCard {jokeResponse} headOnly={false}></JokeCard>
	<br/>
	<a href="/" rel="external">back to home</a>
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
