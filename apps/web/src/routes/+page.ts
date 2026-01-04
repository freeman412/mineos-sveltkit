import type { PageLoad } from './$types';
import { getHostMetrics, getHostServers } from '$lib/api/client';

export const load: PageLoad = async ({ fetch }) => {
	const [metrics, servers] = await Promise.all([getHostMetrics(fetch), getHostServers(fetch)]);

	return {
		metrics,
		servers
	};
};
