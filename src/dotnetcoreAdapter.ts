import esbuild from 'esbuild';
import {
    readFileSync
} from 'fs';
import { join } from 'path';
import { fileURLToPath, URL } from 'url';
import { Adapter, Builder } from '@sveltejs/kit';


type BuilderFix = Builder & {
    utils: {
        log: {
            minor: (a:string) => ""
        },
        copy: (a:string, b:string) => "",
        copy_client_files: (a: string) => "",
        copy_static_files: (a: string) => "",
    },
    config: {
        kit: {
            appDir: string
        }
    }
} 

type esBuildOptions = esbuild.BuildOptions;

type adapterOptions = {
    out: string,
    precompress: boolean,
    env?: {
        host?: string,
        port?: string
    },
    esbuildOptsFunc?: (defaultOptions: esBuildOptions) => Promise<esBuildOptions>
}

export default function ({
    out = 'build',
    //precompress = false, //compression will be done in dotnetcore for performance
    esbuildOptsFunc = null
}: adapterOptions): Adapter {

    const adapter: Adapter = {
        name: '@sveltejs/adapter-dotnetcore',
        adapt: async (builder: BuilderFix): Promise<void> => {
            //utils.update_ignores({ patterns: [out] });
            builder.utils.log.minor('Copying assets')
            const static_directory = join(out, 'assets')

            //utils.copy_client_files(static_directory);
            //utils.copy_static_files(static_directory);
            builder.utils.copy_client_files(static_directory)
            builder.utils.copy_static_files(static_directory)

            builder.utils.log.minor('Building server');
            //const files = fileURLToPath(new URL('./files', import.meta.url));
            const files = fileURLToPath(new URL('./', import.meta.url));
            builder.utils.copy(files, '.svelte-kit/dotnetcore');

            const defaultOptions: esBuildOptions = {
                //entryPoints: ['.svelte-kit/node/index.js'],
                entryPoints: ['.svelte-kit/dotnetcore/index.js'],
                outfile: join(out, 'index.cjs'),
                bundle: true,
                external: Object.keys(JSON.parse(readFileSync('package.json', 'utf8')).dependencies || {}),
                format: 'cjs',
                platform: 'node',
                target: 'node12',
                inject: [join(files, 'shims.js')],
                define: {
                    //esbuild_app_dir: '"' + config.kit.appDir + '"'
                    esbuild_app_dir: '"' + builder.config.kit.appDir + '"'
                }
            };
            const buildOptions: esBuildOptions = esbuildOptsFunc ? await esbuildOptsFunc(defaultOptions) : defaultOptions;
            await esbuild.build(buildOptions);

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

    return adapter;
}
