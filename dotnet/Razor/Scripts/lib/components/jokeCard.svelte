<script lang="ts">
import type { JokeApiResponse } from "../jokeService";

export let jokeResponse: JokeApiResponse
export let headOnly: boolean
</script>

{#if !headOnly}
<section class="joke-card"> 
	{#if jokeResponse.type == 'twopart'}
	<h1>{jokeResponse.setup}</h1>

	<br />

	<div>{jokeResponse.delivery}</div>
	{:else}
	<h1>{jokeResponse.joke}</h1>
	<p></p>
	{/if}
</section>
{/if}

<style lang="scss">
	section.joke-card {
		font-family: sans-serif;
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		margin: 0 auto;
		flex: 1;
		background: url("http://placeimg.com/480/320/people/grayscale") no-repeat center center;
		background-blend-mode: soft-light;
		background-color:antiquewhite;
		max-width: 30em;
		min-width: 30em;
		min-height: 20em;
		max-height: 20em;
		box-shadow: 0 4em 3em -20px #999;
		border-radius: 1em;

		h1 {
			margin: 0 1em 1em;
			line-height: 1.6em;
		}
	}
</style>
