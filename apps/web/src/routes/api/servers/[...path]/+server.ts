import { PRIVATE_API_BASE_URL, PRIVATE_API_KEY } from '$env/static/private';
import type { RequestHandler } from './$types';

const baseUrl = PRIVATE_API_BASE_URL || 'http://localhost:5078';
const apiKey = PRIVATE_API_KEY || '';

export const GET: RequestHandler = async ({ params, url, request, cookies }) => {
	const token = cookies.get('auth_token');
	const headers: HeadersInit = {
		'Content-Type': 'application/json'
	};

	if (apiKey) {
		headers['X-Api-Key'] = apiKey;
	}

	if (token) {
		headers['Authorization'] = `Bearer ${token}`;
	}

	const response = await fetch(
		`${baseUrl}/api/v1/servers/${params.path}${url.search}`,
		{
			method: 'GET',
			headers
		}
	);

	const data = await response.text();
	return new Response(data, {
		status: response.status,
		headers: { 'Content-Type': 'application/json' }
	});
};

export const POST: RequestHandler = async ({ params, url, request, cookies }) => {
	const token = cookies.get('auth_token');
	const headers: HeadersInit = {
		'Content-Type': 'application/json'
	};

	if (apiKey) {
		headers['X-Api-Key'] = apiKey;
	}

	if (token) {
		headers['Authorization'] = `Bearer ${token}`;
	}

	const body = await request.text();

	const response = await fetch(
		`${baseUrl}/api/v1/servers/${params.path}${url.search}`,
		{
			method: 'POST',
			headers,
			body
		}
	);

	const data = await response.text();
	return new Response(data, {
		status: response.status,
		headers: { 'Content-Type': 'application/json' }
	});
};

export const PUT: RequestHandler = async ({ params, url, request, cookies }) => {
	const token = cookies.get('auth_token');
	const headers: HeadersInit = {
		'Content-Type': 'application/json'
	};

	if (apiKey) {
		headers['X-Api-Key'] = apiKey;
	}

	if (token) {
		headers['Authorization'] = `Bearer ${token}`;
	}

	const body = await request.text();

	const response = await fetch(
		`${baseUrl}/api/v1/servers/${params.path}${url.search}`,
		{
			method: 'PUT',
			headers,
			body
		}
	);

	const data = await response.text();
	return new Response(data, {
		status: response.status,
		headers: { 'Content-Type': 'application/json' }
	});
};

export const DELETE: RequestHandler = async ({ params, url, request, cookies }) => {
	const token = cookies.get('auth_token');
	const headers: HeadersInit = {
		'Content-Type': 'application/json'
	};

	if (apiKey) {
		headers['X-Api-Key'] = apiKey;
	}

	if (token) {
		headers['Authorization'] = `Bearer ${token}`;
	}

	const response = await fetch(
		`${baseUrl}/api/v1/servers/${params.path}${url.search}`,
		{
			method: 'DELETE',
			headers
		}
	);

	const data = await response.text();
	return new Response(data, {
		status: response.status,
		headers: { 'Content-Type': 'application/json' }
	});
};
