import type { RequestHandler } from './$types';
import { proxyEventStream } from '$lib/server/streamProxy';

export const GET: RequestHandler = (event) =>
	proxyEventStream(event, `/api/v1/jobs/${encodeURIComponent(event.params.jobId)}/stream`);
