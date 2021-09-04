import { test } from 'uvu';
import * as assert from 'uvu/assert';

// const { PORT = 3000 } = process.env;
// const DEFAULT_SERVER_OPTS = { render: () => {} };

test('to be done', async () => {
	const render = () => {
		return {
			headers: 'wow',
			status: 203,
			body: 'ok'
		};
	}
	assert.equal(render().status, 203);
});

test.run();
