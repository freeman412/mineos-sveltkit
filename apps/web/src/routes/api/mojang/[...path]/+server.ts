import { createProxyHandlers } from '$lib/server/proxyApi';

export const { GET } = createProxyHandlers('/api/v1/mojang');
