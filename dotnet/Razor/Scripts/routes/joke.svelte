<script context="module" lang="ts">
	import type {JokeApiResponse} from '../lib/jokeService'
	import JokeCard from '../lib/components/jokeCard.svelte'
</script>

<script lang="ts">

//[x]: Shadow endpoints joke.ts to receive data
export let jokeResponse: JokeApiResponse

</script>

<JokeCard {jokeResponse}></JokeCard>
