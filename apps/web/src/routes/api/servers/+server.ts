import { createProxyHandlers } from '$lib/server/proxyApi';

export const { POST } = createProxyHandlers('/api/v1/servers');
