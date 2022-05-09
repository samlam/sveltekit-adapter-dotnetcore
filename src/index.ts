import { WriteStream, createWriteStream } from 'fs'
import { Server } from 'SERVER'; /// reference will be replaced during sveltekit build
import { manifest } from 'MANIFEST'; /// reference will be replaced during sveltekit build
import { installFetch } from '@sveltejs/kit/install-fetch'
import { cleanup } from './cleanup'
import { JeringNodeRequest, setupSvelteRequestOptions } from './common'
import { RequestOptions } from '@sveltejs/kit/types/private';

let _logger: WriteStream = null

const _server = new Server(manifest)
const _isDebug: boolean = process.env.NODE_ENV === 'development'

installFetch()

if (_isDebug) {
    const logPath: string = __dirname + '/debug.log'
    console.info(`log path : ${logPath}`)
    _logger = createWriteStream(logPath, {flags : 'w'})
}

const setupRequest = (origRequest: JeringNodeRequest): Request => {
    const {path:url, method, headers, body} = origRequest
    const reqInit: RequestInit = {
        method,
        headers: new Headers(headers),
    }

	if (body) {
		reqInit.body = Buffer.from(body, 'utf-8')
	}

    return new Request(`https://${origRequest.host}${url}` , reqInit)
}

const handleError = (msg:string ) => {
    if (_isDebug)
        _logger.write(msg)
    throw new Error(msg)
}

/**
 * handler for Jering
 * @param callback Jering callback to pass response to
 * @param origRequest Jering request
 */
const HttpHandler = (
    callback: (err: Error, output: {status:number, headers: Headers , body:string}| string) => void, 
    origRequest: JeringNodeRequest
): void=> {
    let req: Request

    try {
        // eslint-disable-next-line no-debugger
        debugger;
        if (_isDebug) {
            _logger.write(`INFO: svelte request payload - ${JSON.stringify(origRequest)} \r\n`)
        }

        try {
            req = setupRequest(origRequest)
            //getRequest(get_origin(req.headers), req as IncomingMessage).then((r) => req = r)
        }
        catch(reqErr) {
            handleError( `ERROR: setupRequest - ${JSON.stringify(reqErr)}`)
        }

        if(!_server)
            handleError(`ERROR: svelte server is null`)

        const svelteReqOption: RequestOptions = setupSvelteRequestOptions(req)
        _server.respond(req, svelteReqOption)
            .then((resp: Response | {body:string}) => {

                if (_isDebug) {
                    _logger.write(`svelte response - ${JSON.stringify(resp)} \r\n`)
                }
                if (origRequest.bodyOnlyReply)
                    callback(null, (resp as {body:string}).body )
                else {
                    const r = (resp as Response)
                    r.text().then((data) => {
                        //HACK: need to refactor to take advantage of the data stream
                        callback(null, {
                            status: r.status,
                            headers: r.headers,
                            body: data
                        })
                    })
                }
            })
            .catch((err:Error) => {
                callback(err, null)
            });
    } catch (err) {
        callback(err, null)
    }
};

cleanup(_logger)

export default HttpHandler
