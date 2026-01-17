import { createProxyHandlers } from '$lib/server/proxyApi';

export const { GET, POST, PATCH, DELETE } = createProxyHandlers('/api/v1/notifications');
