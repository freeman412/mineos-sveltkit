export type DiskMetrics = {
	availableBytes: number;
	freeBytes: number;
	totalBytes: number;
};

export type HostMetrics = {
	uptimeSeconds: number;
	freeMemBytes: number;
	loadAvg: number[];
	disk: DiskMetrics;
};

export type ServerSummary = {
	name: string;
	up: boolean;
	profile?: string | null;
	port?: number | null;
	playersOnline?: number | null;
	playersMax?: number | null;
};

export type ApiResult<T> = {
	data: T | null;
	error: string | null;
};
