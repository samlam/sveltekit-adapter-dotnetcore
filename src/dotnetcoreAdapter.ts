import * as esbuild from 'esbuild'
import {
    readFileSync,
    writeFileSync,
} from 'fs'
import { join } from 'path'
import { fileURLToPath, URL } from 'url'
import type { Adapter, Builder } from '@sveltejs/kit'
import type { AdapterOptionsExtra } from './common'

const adapterfiles: string = fileURLToPath(new URL('./', import.meta.url).href)

export default function ({
    out = 'build',
    //precompress = false, //compression will be done in dotnetcore for performance,
    envPrefix = '',
    esbuildOptsFunc = null
}: AdapterOptionsExtra): Adapter {

    const adapter: Adapter = {
        name: '@sveltejs/adapter-dotnetcore',
        adapt: async (builder:Builder): Promise<void> => {
            builder.rimraf(out)
            builder.log.minor('Copying assets')

            builder.copy(adapterfiles, out, {
                //[x]: can't replace the references here, the file has to be in
                // cjs format 
                // replace: { SERVER: './server/index.js', MANIFEST:
                // './manifest.js', ENV_PREFIX: JSON.stringify(envPrefix)
                // }
            });

            builder.writeClient(`${out}/client`);
            builder.writeStatic(`${out}/client`);
            builder.writeServer(`${out}/server`);
            builder.writePrerendered(`${out}/prerendered`);

            builder.log.warn('replacing references')

            const resultAfterReplace = readFileSync(`${out}/index.js`, {encoding:'utf8'})
                .replace(/'SERVER'/g, `'./server/index.js'`)
                .replace(/'MANIFEST'/g, `'./server/manifest.js'`)
            writeFileSync(`${out}/index.js`, resultAfterReplace, {encoding:'utf8'});

            builder.log.minor('Building server')

            const defaultOptions: esbuild.BuildOptions = {
                entryPoints: [`${out}/index.js`],
                outfile: join(out, 'index.cjs'),
                bundle: true,
                external: Object.keys(JSON.parse(readFileSync('package.json', 'utf8')).dependencies || {}),
                format: 'cjs',
                platform: 'node',
                target: 'node16',
                inject: [join(adapterfiles, 'shims.js')],
                define: {
                    esbuild_app_dir: '"' + builder.config.kit.appDir + '"'
                }
            }

            const buildOptions: esbuild.BuildOptions = esbuildOptsFunc ? await esbuildOptsFunc(defaultOptions) : defaultOptions;
            esbuild.buildSync(buildOptions)

            // TBD - Add prerender here; prerendering requires a live dotnetcore 
            //       server, need to put a bit of thought how it should be setup
            //
            //utils.log.minor('Prerendering static pages');
            // await utils.prerender({
            //     dest: `${out}/prerendered`
            // });
            // if (precompress && existsSync(`${out}/prerendered`)) {
            //     utils.log.minor('Compressing prerendered pages');
            //     await compress(`${out}/prerendered`);
            // }
        }
    };

    return adapter
}
