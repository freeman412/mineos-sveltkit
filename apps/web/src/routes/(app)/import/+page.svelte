<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let actionLoading = $state<Record<string, boolean>>({});
	let serverNames = $state<Record<string, string>>({});

	function formatSize(bytes: number): string {
		if (bytes === 0) return '0 B';
		const k = 1024;
		const sizes = ['B', 'KB', 'MB', 'GB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));
		return `${(bytes / Math.pow(k, i)).toFixed(2)} ${sizes[i]}`;
	}

	function formatDate(date: string): string {
		return new Date(date).toLocaleString();
	}

	async function handleCreate(filename: string) {
		const serverName = (serverNames[filename] || '').trim();
		if (!serverName) {
			alert('Server name is required');
			return;
		}

		actionLoading[filename] = true;
		try {
			const res = await fetch(
				`/api/host/imports/${encodeURIComponent(filename)}/create-server`,
				{
					method: 'POST',
					headers: { 'Content-Type': 'application/json' },
					body: JSON.stringify({ serverName })
				}
			);

			if (!res.ok) {
				const error = await res.json().catch(() => ({ error: 'Failed to import server' }));
				alert(error.error || 'Failed to import server');
			} else {
				serverNames[filename] = '';
				serverNames = { ...serverNames };
				await invalidateAll();
			}
		} finally {
			delete actionLoading[filename];
			actionLoading = { ...actionLoading };
		}
	}
</script>

<div class="page-header">
	<div>
		<h1>Import Servers</h1>
		<p class="subtitle">Create servers from uploaded archives</p>
	</div>
</div>

{#if data.imports.error}
	<div class="error-box">
		<p>Failed to load imports: {data.imports.error}</p>
	</div>
{:else if data.imports.data && data.imports.data.length > 0}
	<div class="imports-card">
		<table>
			<thead>
				<tr>
					<th>Filename</th>
					<th>Size</th>
					<th>Uploaded</th>
					<th>Server Name</th>
					<th>Action</th>
				</tr>
			</thead>
			<tbody>
				{#each data.imports.data as entry}
					<tr>
						<td class="mono">{entry.filename}</td>
						<td>{formatSize(entry.size)}</td>
						<td>{formatDate(entry.time)}</td>
						<td>
							<input
								type="text"
								placeholder="new-server"
								value={serverNames[entry.filename] ?? ''}
								oninput={(event) => {
									const value = (event.currentTarget as HTMLInputElement).value;
									serverNames[entry.filename] = value;
									serverNames = { ...serverNames };
								}}
							/>
						</td>
						<td>
							<button
								class="btn-action"
								onclick={() => handleCreate(entry.filename)}
								disabled={actionLoading[entry.filename]}
							>
								{actionLoading[entry.filename] ? 'Creating...' : 'Create'}
							</button>
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
{:else}
	<div class="empty-state">
		<h2>No imports found</h2>
		<p>Add archives to the import folder to create servers.</p>
	</div>
{/if}

<style>
	.page-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 24px;
	}

	h1 {
		margin: 0 0 8px;
		font-size: 32px;
		font-weight: 600;
	}

	.subtitle {
		margin: 0;
		color: #aab2d3;
		font-size: 15px;
	}

	.imports-card {
		background: #1a1e2f;
		border-radius: 16px;
		padding: 20px;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
		overflow-x: auto;
	}

	table {
		width: 100%;
		border-collapse: collapse;
	}

	th,
	td {
		padding: 12px 10px;
		text-align: left;
		border-bottom: 1px solid #2b2f45;
		color: #d4d9f1;
		font-size: 14px;
	}

	th {
		color: #9aa2c5;
		font-weight: 600;
	}

	.mono {
		font-family: 'Courier New', monospace;
		font-size: 12px;
	}

	input {
		background: #141827;
		border: 1px solid #2a2f47;
		border-radius: 8px;
		padding: 8px 10px;
		color: #eef0f8;
		font-family: inherit;
		font-size: 13px;
		width: 100%;
		box-sizing: border-box;
	}

	.btn-action {
		background: #2b2f45;
		color: #d4d9f1;
		border: none;
		border-radius: 6px;
		padding: 8px 14px;
		font-size: 13px;
		cursor: pointer;
	}

	.btn-action:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.error-box {
		background: rgba(255, 92, 92, 0.1);
		border: 1px solid rgba(255, 92, 92, 0.3);
		border-radius: 12px;
		padding: 16px 20px;
		color: #ff9f9f;
	}

	.empty-state {
		text-align: center;
		padding: 60px 20px;
		color: #8e96bb;
	}
</style>
