// import cookie from 'cookie'
// import { v4 as uuid } from '@lukeed/uuid'
import type { Handle } from '@sveltejs/kit'
//import { RequestEvent } from '@sveltejs/kit/types/internal'

export const handle: Handle = async ({ event, resolve }
	// : { event:RequestEvent<Record<string,string>>, resolve:(RequestEvent<Record<string,string>>, ResolveOptions) => MaybePromise<Response>}
	)=> {

	//console.assert(request, `hook handle req: ${JSON.stringify(request)}`)

	// const cookies = cookie.parse(request.headers.cookie || '')
	// request.locals.userid = cookies.userid || uuid()
	
	// TODO https://github.com/sveltejs/kit/issues/1046
	// if (request.query.has('_method')) {
	// 	request.method = request.query.get('_method').toUpperCase()
	// }
	
	const response:Response = await resolve(event)
	// console.warn('hook', response)

	// if (!cookies.userid) {
	// 	if this is the first time the user has visited this app,
	// 	set a cookie so that we recognise them when they return
	// 	response.headers['set-cookie'] = `userid=${request.locals.userid}; Path=/; HttpOnly`
	// }

	return response;
};
