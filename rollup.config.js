import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import json from '@rollup/plugin-json';
import typescript from '@rollup/plugin-typescript';

var builtinModules = import('module').builtinModules;

export default [
	{
		input: 'src/index.ts',
		output: {
			file: 'files/index.js',
			format: 'cjs',
			sourcemap: true,
			exports:'default'
		},
		plugins: [nodeResolve(), typescript(), commonjs(), json()],
		external: ['../output/server/app.js', './env.js', builtinModules]
	},
	{
		input: 'src/dotnetcoreAdapter.ts',
		output: {
			file: 'files/adapter.js',
			format: 'es',
			sourcemap: true,
			//exports:'default'
		},
		plugins: [nodeResolve(), typescript(), commonjs(), json()],
		external: ['esbuild', 'fs', 'path', builtinModules]
	},
	{
		input: 'src/shims.ts',
		output: {
			file: 'files/shims.js',
			format: 'es'
		},
		plugins: [nodeResolve(), typescript(), commonjs(), json()],
		external: [builtinModules]
	}
];
