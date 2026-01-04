import { PUBLIC_API_BASE_URL } from '$env/static/public';
import type { ApiResult, HostMetrics, ServerSummary } from './types';

type Fetcher = (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;

const defaultBaseUrl = 'http://localhost:5000';

function getBaseUrl() {
	if (PUBLIC_API_BASE_URL && PUBLIC_API_BASE_URL.length > 0) {
		return PUBLIC_API_BASE_URL;
	}
	return defaultBaseUrl;
}

async function apiFetch<T>(fetcher: Fetcher, path: string, init?: RequestInit): Promise<ApiResult<T>> {
	const url = new URL(path, getBaseUrl()).toString();
	try {
		const res = await fetcher(url, init);
		if (!res.ok) {
			return { data: null, error: `Request failed with ${res.status}` };
		}
		const data = (await res.json()) as T;
		return { data, error: null };
	} catch (err) {
		const message = err instanceof Error ? err.message : 'Unknown error';
		return { data: null, error: message };
	}
}

export function getHostMetrics(fetcher: Fetcher) {
	return apiFetch<HostMetrics>(fetcher, '/api/v1/host/metrics');
}

export function getHostServers(fetcher: Fetcher) {
	return apiFetch<ServerSummary[]>(fetcher, '/api/v1/host/servers');
}
