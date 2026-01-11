<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	const formatBytes = (bytes: number): string => {
		if (bytes === 0) return '0 B';
		const k = 1024;
		const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));
		return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
	};

	const formatUptime = (seconds: number): string => {
		const days = Math.floor(seconds / 86400);
		const hours = Math.floor((seconds % 86400) / 3600);
		const minutes = Math.floor((seconds % 3600) / 60);

		if (days > 0) return `${days}d ${hours}h`;
		if (hours > 0) return `${hours}h ${minutes}m`;
		return `${minutes}m`;
	};

	const totalServers = $derived(data.servers.data?.length ?? 0);
	const runningServers = $derived(
		data.servers.data?.filter((s) => s.up).length ?? 0
	);
	const totalPlayers = $derived(
		data.servers.data?.reduce((sum, s) => sum + (s.playersOnline ?? 0), 0) ?? 0
	);
	const maxPlayers = $derived(
		data.servers.data?.reduce((sum, s) => sum + (s.playersMax ?? 0), 0) ?? 0
	);
</script>

<div class="dashboard">
	<div class="page-header">
		<div>
			<h1>Dashboard</h1>
			<p class="subtitle">Overview of your Minecraft hosting environment</p>
		</div>
		<a href="/servers/new" class="btn-primary">+ Create Server</a>
	</div>

	<!-- Quick Stats -->
	<div class="stats-grid">
		<div class="stat-card">
			<div class="stat-icon">🖥️</div>
			<div class="stat-content">
				<div class="stat-value">{totalServers}</div>
				<div class="stat-label">Total Servers</div>
			</div>
		</div>

		<div class="stat-card">
			<div class="stat-icon">✅</div>
			<div class="stat-content">
				<div class="stat-value">{runningServers}</div>
				<div class="stat-label">Running</div>
			</div>
		</div>

		<div class="stat-card">
			<div class="stat-icon">👥</div>
			<div class="stat-content">
				<div class="stat-value">{totalPlayers} / {maxPlayers}</div>
				<div class="stat-label">Players Online</div>
			</div>
		</div>

		{#if data.hostMetrics.data}
			<div class="stat-card">
				<div class="stat-icon">💾</div>
				<div class="stat-content">
					<div class="stat-value">
						{formatBytes(data.hostMetrics.data.disk.freeBytes)}
					</div>
					<div class="stat-label">Disk Free</div>
				</div>
			</div>
		{/if}
	</div>

	<!-- Host Metrics -->
	{#if data.hostMetrics.data}
		<div class="metrics-section">
			<h2>Host Metrics</h2>
			<div class="metrics-grid">
				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-title">System Uptime</span>
					</div>
					<div class="metric-value">
						{formatUptime(data.hostMetrics.data.uptimeSeconds)}
					</div>
				</div>

				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-title">Load Average</span>
					</div>
					<div class="metric-value">
						{data.hostMetrics.data.loadAvg
							.slice(0, 3)
							.map((v) => v.toFixed(2))
							.join(', ')}
					</div>
				</div>

				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-title">Free Memory</span>
					</div>
					<div class="metric-value">
						{formatBytes(data.hostMetrics.data.freeMemBytes)}
					</div>
				</div>

				<div class="metric-card">
					<div class="metric-header">
						<span class="metric-title">Disk Usage</span>
					</div>
					<div class="metric-value">
						{((
							(data.hostMetrics.data.disk.totalBytes -
								data.hostMetrics.data.disk.freeBytes) /
							data.hostMetrics.data.disk.totalBytes
						) * 100).toFixed(1)}%
					</div>
					<div class="metric-subtext">
						{formatBytes(
							data.hostMetrics.data.disk.totalBytes - data.hostMetrics.data.disk.freeBytes
						)} / {formatBytes(data.hostMetrics.data.disk.totalBytes)}
					</div>
				</div>
			</div>
		</div>
	{/if}

	<!-- Recent Servers -->
	<div class="servers-section">
		<div class="section-header">
			<h2>Servers</h2>
			<a href="/servers" class="link-btn">View all →</a>
		</div>

		{#if data.servers.error}
			<div class="error-box">
				<p>Failed to load servers: {data.servers.error}</p>
			</div>
		{:else if data.servers.data && data.servers.data.length > 0}
			<div class="server-list">
				{#each data.servers.data.slice(0, 6) as server}
					<a href="/servers/{server.name}" class="server-item">
						<div class="server-info">
							<div class="server-name">{server.name}</div>
							<div class="server-meta">
								{#if server.profile}
									<span class="profile-badge">{server.profile}</span>
								{/if}
								{#if server.port}
									<span class="port-badge">:{server.port}</span>
								{/if}
							</div>
						</div>
						<div class="server-status">
							{#if server.up}
								<span class="status-indicator status-up"></span>
								<span class="status-text">Running</span>
								{#if server.playersOnline !== null && server.playersMax !== null}
									<span class="players-count"
										>{server.playersOnline}/{server.playersMax}</span
									>
								{/if}
							{:else}
								<span class="status-indicator status-down"></span>
								<span class="status-text">Stopped</span>
							{/if}
						</div>
					</a>
				{/each}
			</div>
		{:else}
			<div class="empty-state">
				<p class="empty-icon">📦</p>
				<h3>No servers yet</h3>
				<p>Create your first Minecraft server to get started</p>
				<a href="/servers/new" class="btn-primary">Create Server</a>
			</div>
		{/if}
	</div>

	<!-- Quick Links -->
	<div class="quick-links">
		<a href="/profiles" class="quick-link-card">
			<div class="quick-link-icon">📦</div>
			<div class="quick-link-content">
				<div class="quick-link-title">Profiles</div>
				<div class="quick-link-desc">Download and manage server JARs</div>
			</div>
		</a>

		<a href="/import" class="quick-link-card">
			<div class="quick-link-icon">📥</div>
			<div class="quick-link-content">
				<div class="quick-link-title">Import</div>
				<div class="quick-link-desc">Import servers from archives</div>
			</div>
		</a>
	</div>
</div>

<style>
	.dashboard {
		max-width: 1200px;
	}

	.page-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 32px;
		gap: 24px;
	}

	h1 {
		margin: 0 0 8px;
		font-size: 32px;
		font-weight: 600;
	}

	h2 {
		margin: 0 0 16px;
		font-size: 20px;
		font-weight: 600;
	}

	h3 {
		margin: 0 0 8px;
		font-size: 18px;
		font-weight: 600;
	}

	.subtitle {
		margin: 0;
		color: #aab2d3;
		font-size: 15px;
	}

	.btn-primary {
		background: #5865f2;
		color: white;
		border: none;
		border-radius: 8px;
		padding: 12px 24px;
		font-family: inherit;
		font-size: 15px;
		font-weight: 600;
		cursor: pointer;
		transition: background 0.2s;
		text-decoration: none;
		display: inline-block;
	}

	.btn-primary:hover {
		background: #4752c4;
	}

	.stats-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 20px;
		margin-bottom: 32px;
	}

	.stat-card {
		background: #1a1e2f;
		border-radius: 16px;
		padding: 24px;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
		display: flex;
		align-items: center;
		gap: 16px;
	}

	.stat-icon {
		font-size: 32px;
		opacity: 0.9;
	}

	.stat-content {
		flex: 1;
	}

	.stat-value {
		font-size: 28px;
		font-weight: 600;
		color: #eef0f8;
		line-height: 1;
		margin-bottom: 4px;
	}

	.stat-label {
		font-size: 13px;
		color: #9aa2c5;
	}

	.metrics-section {
		margin-bottom: 32px;
	}

	.metrics-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 16px;
	}

	.metric-card {
		background: #1a1e2f;
		border-radius: 12px;
		padding: 20px;
		box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
	}

	.metric-header {
		margin-bottom: 12px;
	}

	.metric-title {
		font-size: 13px;
		color: #9aa2c5;
		text-transform: uppercase;
		letter-spacing: 0.5px;
		font-weight: 500;
	}

	.metric-value {
		font-size: 24px;
		font-weight: 600;
		color: #eef0f8;
	}

	.metric-subtext {
		font-size: 12px;
		color: #8890b1;
		margin-top: 4px;
	}

	.servers-section {
		margin-bottom: 32px;
	}

	.section-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 16px;
	}

	.link-btn {
		color: #5865f2;
		text-decoration: none;
		font-size: 14px;
		font-weight: 500;
		transition: color 0.2s;
	}

	.link-btn:hover {
		color: #4752c4;
	}

	.server-list {
		background: #1a1e2f;
		border-radius: 16px;
		overflow: hidden;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
	}

	.server-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 16px 20px;
		border-bottom: 1px solid #2a2f47;
		text-decoration: none;
		transition: background 0.2s;
	}

	.server-item:last-child {
		border-bottom: none;
	}

	.server-item:hover {
		background: rgba(88, 101, 242, 0.05);
	}

	.server-info {
		display: flex;
		flex-direction: column;
		gap: 6px;
	}

	.server-name {
		font-size: 16px;
		font-weight: 500;
		color: #eef0f8;
	}

	.server-meta {
		display: flex;
		gap: 8px;
		align-items: center;
	}

	.profile-badge {
		font-size: 12px;
		color: #9aa2c5;
		background: rgba(154, 162, 197, 0.1);
		padding: 2px 8px;
		border-radius: 4px;
	}

	.port-badge {
		font-size: 12px;
		color: #9aa2c5;
		font-family: 'Courier New', monospace;
	}

	.server-status {
		display: flex;
		align-items: center;
		gap: 8px;
	}

	.status-indicator {
		width: 8px;
		height: 8px;
		border-radius: 50%;
	}

	.status-up {
		background: #7ae68d;
		box-shadow: 0 0 8px rgba(122, 230, 141, 0.4);
	}

	.status-down {
		background: #ff9f9f;
	}

	.status-text {
		font-size: 14px;
		color: #d4d9f1;
	}

	.players-count {
		font-size: 13px;
		color: #9aa2c5;
		margin-left: 4px;
	}

	.error-box {
		background: rgba(255, 92, 92, 0.1);
		border: 1px solid rgba(255, 92, 92, 0.3);
		border-radius: 12px;
		padding: 16px 20px;
		color: #ff9f9f;
	}

	.error-box p {
		margin: 0;
	}

	.empty-state {
		text-align: center;
		padding: 60px 20px;
		color: #8e96bb;
	}

	.empty-icon {
		font-size: 48px;
		margin-bottom: 12px;
	}

	.quick-links {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
		gap: 20px;
	}

	.quick-link-card {
		background: #1a1e2f;
		border-radius: 16px;
		padding: 24px;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
		display: flex;
		align-items: center;
		gap: 16px;
		text-decoration: none;
		transition: all 0.2s;
	}

	.quick-link-card:hover {
		transform: translateY(-2px);
		box-shadow: 0 24px 48px rgba(0, 0, 0, 0.45);
	}

	.quick-link-icon {
		font-size: 32px;
		opacity: 0.9;
	}

	.quick-link-content {
		flex: 1;
	}

	.quick-link-title {
		font-size: 16px;
		font-weight: 600;
		color: #eef0f8;
		margin-bottom: 4px;
	}

	.quick-link-desc {
		font-size: 13px;
		color: #9aa2c5;
	}

	@media (max-width: 768px) {
		.page-header {
			flex-direction: column;
			align-items: flex-start;
		}

		.stats-grid {
			grid-template-columns: repeat(2, 1fr);
		}

		.metrics-grid {
			grid-template-columns: 1fr;
		}

		.quick-links {
			grid-template-columns: 1fr;
		}
	}
</style>
