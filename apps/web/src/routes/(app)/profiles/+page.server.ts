import type { PageServerLoad } from './$types';
import { getHostProfiles, getHostServers } from '$lib/api/client';

export const load: PageServerLoad = async ({ fetch }) => {
	const profiles = await getHostProfiles(fetch);
	const servers = await getHostServers(fetch);

	return {
		profiles,
		servers
	};
};
