import esbuild from 'esbuild'

export type JeringNodeRequest = {
    method: string,
    headers: Record<string, string>,
    host: string,
    path: string,
    body: string,
    query: URLSearchParams,
    queryString: string,
    rawBody: Uint8Array | null,
    bodyOnlyReply: boolean //this can improve the performance in some situations
}

export type JeringNodeResponse = {
    status:number, 
    headers: Headers , 
    body:string
}


/**
 * defined in adapter-node
 * <https://github.com/sveltejs/kit/blob/37520a3d2873ba941288aa2e3bfb3877ad7688e2/packages/adapter-node/index.d.ts#L7-L11>
 */
export interface AdapterOptions {
	out?: string;
	precompress?: boolean;
	envPrefix?: string;
}

export type AdapterOptionsExtra = AdapterOptions & {
    esbuildOptsFunc?: (defaultOptions: esbuild.BuildOptions) => Promise<esbuild.BuildOptions>
}


// eslint-disable-next-line @typescript-eslint/no-unused-vars
export const setupSvelteRequestOptions = (req:unknown ) => {
    return {
            getClientAddress() {
            //TODO: need to fix address_header
            // if (address_header) {
            //     const value = /** @type {string} */ (req.headers[address_header]) || '';

            //     if (address_header === 'x-forwarded-for') {
            //         const addresses = value.split(',');

            //         if (xff_depth < 1) {
            //             throw new Error(`${ENV_PREFIX + 'XFF_DEPTH'} must be a positive integer`);
            //         }

            //         if (xff_depth > addresses.length) {
            //             throw new Error(
            //                 `${ENV_PREFIX + 'XFF_DEPTH'} is ${xff_depth}, but only found ${
            //                     addresses.length
            //                 } addresses`
            //             );
            //         }
            //         return addresses[addresses.length - xff_depth].trim();
            //     }

            //     return value;
            // }

            // return (
            //     req.connection?.remoteAddress ||
            //     // @ts-expect-error
            //     req.connection?.socket?.remoteAddress ||
            //     req.socket?.remoteAddress ||
            //     // @ts-expect-error
            //     req.info?.remoteAddress
            // );

            return (
                "127.0.0.1"
            )
        }
    }
}