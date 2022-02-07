import { WriteStream, createWriteStream } from 'fs'
import { URLSearchParams } from 'url'
// @ts-ignore
import { init, render } from '../output/server/app.js'
import { Response } from '@sveltejs/kit/install-fetch'
import { cleanup } from './cleanup'

const _isDebug: boolean = process.env.NODE_ENV === 'development'
const _encoder: TextEncoder = new TextEncoder()
let _logger: WriteStream = null

if (_isDebug) {
    const logPath: string = __dirname + '/debug.log'
    console.info(`log path : ${logPath}`)
    _logger = createWriteStream(logPath, {flags : 'w'})
}

const HttpHandler = (
    callback: (err: Error, output: Response| string) => void, 
    origRequest: {
        method: string,
        headers: Record<string, string>,
        path: string,
        body: string,
        query: URLSearchParams,
        queryString: string,
        rawBody: Uint8Array | null,
        bodyOnlyReply: boolean //this can improve the performance in some situations
}): void=> {
    try {
        init({ paths: { base: '', assets: '/.' }, prerendering: true })
        origRequest.query = new URLSearchParams(origRequest.queryString)
        origRequest.rawBody = _encoder.encode(origRequest.body)

        if (_isDebug) {
            _logger.write(`svelte request payload - ${JSON.stringify(origRequest)} \r\n`)
        }

        render(origRequest)
            .then((resp: Response | {body:string}) => {

                if (_isDebug) {
                    _logger.write(`svelte response - ${JSON.stringify(resp)} \r\n`)
                }
                if (origRequest.bodyOnlyReply)
                    callback(null, (resp as {body:string}).body )
                else
                    // callback(null, {
                    //     status: resp.status,
                    //     headers: resp.headers,
                    //     body: resp.body
                    // })
                    callback(null, resp as Response)
            })
            .catch((err:Error) => callback(err, null));
    } catch (err) {
        callback(err, null)
    }
};

cleanup(_logger)

export default HttpHandler
