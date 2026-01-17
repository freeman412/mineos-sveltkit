import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const usersRes = await fetch('/api/auth/users');
	const users = usersRes.ok ? await usersRes.json() : null;

	return {
		users: {
			data: users,
			error: usersRes.ok ? null : `Failed to load users (${usersRes.status})`
		}
	};
};
