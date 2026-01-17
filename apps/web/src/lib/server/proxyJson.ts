import { env } from '$env/dynamic/private';
import { json } from '@sveltejs/kit';
import type { RequestEvent } from '@sveltejs/kit';

const baseUrl =
	env.PRIVATE_API_BASE_URL ??
	env.INTERNAL_API_URL ??
	'http://api:5078';

const apiKey = env.PRIVATE_API_KEY ?? '';

type ProxyJsonOptions = {
	requireApiKey?: boolean;
	method?: string;
	body?: BodyInit | null;
	headers?: HeadersInit;
};

export async function proxyJson(
	event: RequestEvent,
	upstreamPath: string,
	options: ProxyJsonOptions = {}
) {
	if ((options.requireApiKey ?? true) && !apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const token = event.cookies.get('auth_token');

	const headers: Record<string, string> = {};
	const contentType = event.request.headers.get('content-type');
	if (contentType) headers['Content-Type'] = contentType;
	const accept = event.request.headers.get('accept');
	if (accept) headers['Accept'] = accept;
	if (apiKey) headers['X-Api-Key'] = apiKey;
	if (token) headers['Authorization'] = `Bearer ${token}`;

	if (options.headers) {
		for (const [key, value] of Object.entries(options.headers)) {
			if (typeof value === 'string') {
				headers[key] = value;
			}
		}
	}

	const method = (options.method ?? event.request.method ?? 'GET').toUpperCase();
	let body = options.body ?? null;
	if (!body && method !== 'GET' && method !== 'DELETE') {
		body = await event.request.arrayBuffer();
	}

	const res = await event.fetch(`${baseUrl}${upstreamPath}${event.url.search}`, {
		method,
		headers,
		body: body ?? undefined
	});
	if (res.status === 204 || res.status === 205) {
		return new Response(null, { status: res.status });
	}

	const responseType = res.headers.get('content-type') ?? '';
	const expectsJson =
		responseType.includes('application/json') || responseType.includes('+json');

	if (expectsJson) {
		try {
			const data = await res.json();
			return json(data, { status: res.status });
		} catch {
			// Fall through to text parsing for non-JSON error bodies.
		}
	}

	const text = await res.text();
	if (!text) {
		return json(null, { status: res.status });
	}

	return json({ error: text }, { status: res.status });
}
