import { JSONValue, MaybePromise } from "@sveltejs/kit/types/private"

export type JokeApiResponse = {
    error: boolean
    category: string
    type: `twopart` | `single`
    setup: string
    delivery: string
    joke: string
    flags: JokeFlags
    id: number
    safe: boolean
    lang: string
}
  
export type JokeFlags = {
    nsfw: boolean
    religious: boolean
    political: boolean
    racist: boolean
    sexist: boolean
    explicit: boolean
}


export const getJoke: ()=> MaybePromise<{body?: JSONValue | Uint8Array | unknown;}> = 
    async () => {
    try {
        const jokeReponse = await fetch(`https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,racist,sexist`)

        return {
            body: {
                jokeResponse: await jokeReponse.json()
            }
        }
    } catch (err) {
        return returnError(err, `joke api get error`)
    }
}

const returnError = (err: Error, customMessage: string): 
    {status: number,body: {message: string, stack: string}} => {
    return {
        status: 500,
        body: {
            message: `${customMessage} error occured ${err.message}`,
            stack: err.stack
        }
    }
}