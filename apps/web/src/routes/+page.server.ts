import { redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ cookies }) => {
	const token = cookies.get('auth_token');

	// Redirect to dashboard if authenticated, login if not
	if (token) {
		throw redirect(303, '/dashboard');
	} else {
		throw redirect(303, '/login');
	}
};
