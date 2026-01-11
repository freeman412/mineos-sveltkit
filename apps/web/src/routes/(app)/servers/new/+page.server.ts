import type { PageServerLoad } from './$types';
import * as api from '$lib/api/client';

export const load: PageServerLoad = async ({ fetch }) => {
	const profiles = await api.listProfiles(fetch);

	return {
		profiles
	};
};
