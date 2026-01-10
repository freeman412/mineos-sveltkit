import { PRIVATE_API_BASE_URL, PRIVATE_API_KEY } from '$env/static/private';
import type { RequestHandler } from './$types';

const baseUrl = PRIVATE_API_BASE_URL || 'http://localhost:5078';
const apiKey = PRIVATE_API_KEY || '';

export const GET: RequestHandler = async ({ params }) => {
	if (!apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const response = await fetch(`${baseUrl}/api/v1/jobs/${params.jobId}/stream`, {
		headers: {
			'X-Api-Key': apiKey,
			Accept: 'text/event-stream'
		}
	});

	if (!response.body) {
		return new Response('Upstream did not return a body', { status: 502 });
	}

	const stream = new ReadableStream<Uint8Array>({
		async start(controller) {
			const reader = response.body!.getReader();
			try {
				while (true) {
					const { done, value } = await reader.read();
					if (done) break;
					if (value) controller.enqueue(value);
				}
			} catch (err) {
				controller.error(err);
			} finally {
				reader.releaseLock();
				controller.close();
			}
		}
	});

	const headers = new Headers(response.headers);
	headers.set('Content-Type', 'text/event-stream');
	headers.set('Cache-Control', 'no-cache');
	headers.set('Connection', 'keep-alive');
	headers.set('X-Accel-Buffering', 'no');
	headers.delete('Content-Length');

	return new Response(stream, {
		status: response.status,
		headers
	});
};
