import { PRIVATE_API_BASE_URL, PRIVATE_API_KEY } from '$env/static/private';
import type { RequestHandler } from './$types';

const baseUrl = PRIVATE_API_BASE_URL || 'http://localhost:5078';
const apiKey = PRIVATE_API_KEY || '';

const buildHeaders = (request: Request, token?: string | null) => {
	const headers: HeadersInit = {};
	const contentType = request.headers.get('content-type');
	if (contentType) {
		headers['Content-Type'] = contentType;
	}

	if (apiKey) {
		headers['X-Api-Key'] = apiKey;
	}

	if (token) {
		headers['Authorization'] = `Bearer ${token}`;
	}

	return headers;
};

export const GET: RequestHandler = async ({ params, url, request, cookies }) => {
	if (!apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const token = cookies.get('auth_token');
	const headers = buildHeaders(request, token);

	const response = await fetch(`${baseUrl}/api/v1/host/${params.path}${url.search}`, {
		method: 'GET',
		headers
	});

	return new Response(response.body, {
		status: response.status,
		headers: {
			'Content-Type': response.headers.get('content-type') ?? 'application/json'
		}
	});
};

export const POST: RequestHandler = async ({ params, url, request, cookies }) => {
	if (!apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const token = cookies.get('auth_token');
	const headers = buildHeaders(request, token);
	const body = await request.arrayBuffer();

	const response = await fetch(`${baseUrl}/api/v1/host/${params.path}${url.search}`, {
		method: 'POST',
		headers,
		body
	});

	return new Response(response.body, {
		status: response.status,
		headers: {
			'Content-Type': response.headers.get('content-type') ?? 'application/json'
		}
	});
};

export const PUT: RequestHandler = async ({ params, url, request, cookies }) => {
	if (!apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const token = cookies.get('auth_token');
	const headers = buildHeaders(request, token);
	const body = await request.arrayBuffer();

	const response = await fetch(`${baseUrl}/api/v1/host/${params.path}${url.search}`, {
		method: 'PUT',
		headers,
		body
	});

	return new Response(response.body, {
		status: response.status,
		headers: {
			'Content-Type': response.headers.get('content-type') ?? 'application/json'
		}
	});
};

export const DELETE: RequestHandler = async ({ params, url, request, cookies }) => {
	if (!apiKey) {
		return new Response('Missing PRIVATE_API_KEY', { status: 500 });
	}

	const token = cookies.get('auth_token');
	const headers = buildHeaders(request, token);

	const response = await fetch(`${baseUrl}/api/v1/host/${params.path}${url.search}`, {
		method: 'DELETE',
		headers
	});

	return new Response(response.body, {
		status: response.status,
		headers: {
			'Content-Type': response.headers.get('content-type') ?? 'application/json'
		}
	});
};
