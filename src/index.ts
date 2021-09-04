import { URLSearchParams } from 'url';
// @ts-ignore
import { init, render } from '../output/server/app.js';

const HttpHandler = (callback: (err: Error, output: string) => void, svelteRequest: {
    method: string,
    headers: Record<string, string>,
    path: string,
    rawBody: string,
    query: URLSearchParams,
    queryString: string
}): void=> {
    try {
        init({ paths: { base: '', assets: '/.' }, prerendering: true });
        svelteRequest.query = new URLSearchParams(svelteRequest.queryString);
        
        render(svelteRequest)
            .then((resp: { body: string }) => {
                callback(null, resp.body);
            });
    } catch (err) {
        callback(err, null);
    }
};

export default HttpHandler;
