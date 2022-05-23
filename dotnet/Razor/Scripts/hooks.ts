// import cookie from 'cookie'
// import { v4 as uuid } from '@lukeed/uuid'
import type { Handle } from '@sveltejs/kit'
import fs from 'node:fs'

//import { RequestEvent } from '@sveltejs/kit/types/internal'

const [_beforeHead, _afterHead, _afterBody] = fs.readFileSync('./Scripts/app-page.html', {encoding: 'utf-8'}).split('%section%')

export const handle: Handle = async ({ event, resolve })=> {

	const response = await resolve(event)

	if (!event.request.headers.has('x-component')) {

		const [headText, bodyText] = (await response.text()).split('%section%')

		const finalResponse = new Response([
			_beforeHead,
			headText, 
			_afterHead,
			bodyText,
			_afterBody
		].join('\r\n'), { 
			headers: response.headers,
			status: response.status,
			statusText: response.statusText
		})
		return finalResponse
	}

	const renderedText = await response.text()
	const sanitizedResponse =  (event.request.url.endsWith('-head'))? 
		renderedText.substring(0, renderedText.indexOf('%section%')) :
		renderedText.replace('%section%','')

	return new Response(sanitizedResponse, {
		headers: response.headers,
		status: response.status,
		statusText: response.statusText
	})
};

