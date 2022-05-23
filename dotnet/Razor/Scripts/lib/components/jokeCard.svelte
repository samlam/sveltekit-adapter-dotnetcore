<script lang="ts">
import type { JokeApiResponse } from "../jokeService";

export let jokeResponse: JokeApiResponse
//this removes the content and renders link tags only
export let headOnly: boolean = false
</script>

{#if !headOnly}
<section class="joke-card"> 
	{#if jokeResponse.type == 'twopart'}
	<h1>{jokeResponse.setup}</h1>

	<br />

	<div>{jokeResponse.delivery}</div>
	{:else}
	<h1>{jokeResponse.joke}</h1>

	<br />
	{/if}
	<span class="joke-id">
		<a 
			href="/joke/{jokeResponse.id}" 
			alt="permalink" 
			title="permalink to this joke {jokeResponse.id}"
			rel="external">
				{jokeResponse.id}
		</a>
	</span>
</section>
{/if}

<style lang="scss">
	section.joke-card {
		font-family: sans-serif;
		font-size: 1em;
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
			margin: auto;
			line-height: 1.6em;
			font-size: 1em;
			font-weight: bold;
		}

		.joke-id {
			margin-top: auto;
			margin-left: 25em;
			font-size: 0.8em;
			color: #777;
			a {
				color: #777;
				text-decoration: none;
			}
		}
	}
</style>
