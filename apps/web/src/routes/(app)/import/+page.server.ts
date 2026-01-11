import type { PageServerLoad } from './$types';
import { getHostImports } from '$lib/api/client';

export const load: PageServerLoad = async ({ fetch }) => {
	const imports = await getHostImports(fetch);
	return {
		imports
	};
};
