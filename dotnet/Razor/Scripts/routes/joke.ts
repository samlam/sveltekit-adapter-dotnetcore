import type { JSONValue, MaybePromise } from '@sveltejs/kit/types/private';
import {getJoke} from '../lib/jokeService'

export const get:()=> MaybePromise<{body?: JSONValue | Uint8Array | unknown;}> = async () => await getJoke()
