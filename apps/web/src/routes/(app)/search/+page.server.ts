import type { PageServerLoad } from './$types';
import * as api from '$lib/api/client';

export const load: PageServerLoad = async ({ fetch, url }) => {
	const [servers, profiles] = await Promise.all([
		api.listServers(fetch),
		api.listProfiles(fetch)
	]);

	return {
		servers,
		profiles,
		query: url.searchParams.get('q') ?? ''
	};
};
