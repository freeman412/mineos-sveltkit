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

export type Profile = {
	id: string;
	group: string;
	type: string;
	version: string;
	releaseTime: string;
	url: string;
	filename: string;
	downloaded: boolean;
	progress: unknown | null;
};

export type ArchiveEntry = {
	time: string;
	size: number;
	filename: string;
};

export type ApiResult<T> = {
	data: T | null;
	error: string | null;
};

// Server management types
export type ServerDetail = {
	name: string;
	createdAt: string;
	ownerUid: number;
	ownerGid: number;
	ownerUsername: string;
	ownerGroupname: string;
	status: string;
	javaPid: number | null;
	screenPid: number | null;
	config: ServerConfig | null;
};

export type ServerConfig = {
	java: JavaConfig;
	minecraft: MinecraftConfig;
	onReboot: OnRebootConfig;
};

export type JavaConfig = {
	javaBinary: string;
	javaXmx: number;
	javaXms: number;
	javaTweaks: string | null;
	jarFile: string | null;
	jarArgs: string | null;
};

export type MinecraftConfig = {
	profile: string | null;
	unconventional: boolean;
};

export type OnRebootConfig = {
	start: boolean;
};

export type ServerHeartbeat = {
	name: string;
	status: string;
	javaPid: number | null;
	screenPid: number | null;
	ping: PingInfo | null;
	memoryBytes: number | null;
};

export type PingInfo = {
	protocol: number;
	serverVersion: string;
	motd: string;
	playersOnline: number;
	playersMax: number;
};

export type CreateServerRequest = {
	name: string;
	ownerUid: number;
	ownerGid: number;
};
