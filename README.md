# sveltekit-adapter-dotnetcore

[Adapter](https://kit.svelte.dev/docs#adapters) for SvelteKit apps that runs with standalone dotnetcore server, which requires [jering-nodejs](https://github.com/JeringTech/Javascript.NodeJS).

## Usage

Install with `npm i -D sveltekit-adapter-dotnetcore@next`, then add the adapter to your `svelte.config.js`:

```js
// svelte.config.js
import adapter from 'sveltekit-adapter-dotnetcore';

export default {
  kit: {
    adapter: adapter({
      // default options are shown
      out: 'build'
    })
  }
};
```

### out parameter

The default output directory is set to `build`

## Next or TBD

Will provide a .net core example

## License

[MIT](LICENSE)
