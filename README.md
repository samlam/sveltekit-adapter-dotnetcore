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

## dotnet sample app

* Run pnpm to restore node packages.

```sh
pnpm install
```

* build the adapter

```sh
npm run build
```

* build the sample app

```sh
pnpm run build --filter='sveltekit-dotnet'
```

* to build and debug dotnet application with VS code, by hitting F5 with the
  `.NET Core Launch (Web)` profile; in the sample app, the about page is
  rendered in sveltekit using server side method

The site is running on <https://localhost:5005/>

* for sveltekit HMR, just run `npm run dev` in the Razor folder

The about page is on <http://localhost:3000/about>

## Dependencies

Please note the project is tested with node.js v16, which supports fetch.

It requires `pnpm` to be installed globally.

```sh
npm install -g pnpm
```

## Next or TBD

Will improve aspnetcore Razor example

## License

[MIT](LICENSE)
