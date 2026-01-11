import type { PageServerLoad } from './$types';
import * as api from '$lib/api/client';

export const load: PageServerLoad = async ({ fetch }) => {
	const [servers, hostMetrics] = await Promise.all([
		api.listServers(fetch),
		api.getHostMetrics(fetch)
	]);

	return {
		servers,
		hostMetrics
	};
};
