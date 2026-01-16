import type { RequestHandler } from './$types';
import { proxyJson } from '$lib/server/proxyJson';

export const GET: RequestHandler = (event) =>
	proxyJson(
		event,
		`/api/v1/curseforge/mod/${encodeURIComponent(event.params.id)}/files`,
		{ requireApiKey: false }
	);
