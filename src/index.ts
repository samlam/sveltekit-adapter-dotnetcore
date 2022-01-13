import { URLSearchParams } from 'url';
// @ts-ignore
import { init, render } from '../output/server/app.js';
import { Response } from '@sveltejs/kit/install-fetch';


const HttpHandler = (callback: (err: Error, output: Response| string) => void, svelteRequest: {
    method: string,
    headers: Record<string, string>,
    path: string,
    rawBody: string,
    query: URLSearchParams,
    queryString: string,
    bodyOnlyReply: boolean //this can improve the performance in some situations
}): void=> {
    try {
        init({ paths: { base: '', assets: '/.' }, prerendering: true });
        svelteRequest.query = new URLSearchParams(svelteRequest.queryString);

        render(svelteRequest)
            .then((resp: Response | string) => {
                if (svelteRequest.bodyOnlyReply)
                    callback(null, resp.body);
                else
                    callback(null, {
                        status: resp.status,
                        headers: resp.headers,
                        body: resp.body
                    });
            });
    } catch (err) {
        callback(err, null);
    }
};

export default HttpHandler;
