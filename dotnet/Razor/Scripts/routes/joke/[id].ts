import type { JSONValue, MaybePromise, RequestEvent } from '@sveltejs/kit/types/private';
import {getJoke} from '../../lib/jokeService'

export const get:(request:RequestEvent)=> MaybePromise<{body?: JSONValue | Uint8Array | unknown;}> = 
    async (request) => await getJoke(request.params.id)