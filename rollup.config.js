/* eslint-disable @typescript-eslint/no-var-requires */
import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import json from '@rollup/plugin-json';
import typescript from 'rollup-plugin-typescript2';
import pkg from './package.json';

export default [
	{
		input: 'src/index.ts',
		output: {
			file: 'files/index.js',
			inlineDynamicImports: true,
			format: 'cjs',
		},
		plugins: [nodeResolve( { modulesOnly:true, browser:false}), typescript(), json(), 
			commonjs({strictRequires:true, transformMixedEsModules:false})],
		external: ['SERVER', 'MANIFEST', Object.keys(pkg.devDependencies), ...require('module').builtinModules]
	},
	{
		input: 'src/dotnetcoreAdapter.ts',
		output: {
			file: 'files/adapter.js',
			inlineDynamicImports: true,
			format: 'es',
		},
		plugins: [nodeResolve(), typescript(), json(),commonjs()],
		external: ['esbuild',Object.keys(pkg.devDependencies), ...require('module').builtinModules]
	},
	{
		input: 'src/shims.ts',
		output: {
			file: 'files/shims.js',
			inlineDynamicImports: true,
			format: 'es',
		},
		plugins: [nodeResolve(), typescript()],
		external: [...require('module').builtinModules]
	}
];
