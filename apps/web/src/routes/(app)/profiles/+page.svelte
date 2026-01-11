<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let buildGroup = $state('spigot');
	let buildVersion = $state('');
	let buildError = $state('');
	let actionLoading = $state<Record<string, boolean>>({});
	let copyTargets = $state<Record<string, string>>({});

	async function handleDownload(profileId: string) {
		actionLoading[profileId] = true;
		try {
			const res = await fetch(`/api/host/profiles/${profileId}/download`, { method: 'POST' });
			if (!res.ok) {
				const error = await res.json().catch(() => ({ error: 'Failed to download profile' }));
				alert(error.error || 'Failed to download profile');
			} else {
				await invalidateAll();
			}
		} finally {
			delete actionLoading[profileId];
			actionLoading = { ...actionLoading };
		}
	}

	async function handleCopy(profileId: string) {
		const target = copyTargets[profileId];
		if (!target) {
			alert('Select a server to copy this profile to.');
			return;
		}

		actionLoading[profileId] = true;
		try {
			const res = await fetch(`/api/host/profiles/${profileId}/copy-to-server`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ serverName: target })
			});
			if (!res.ok) {
				const error = await res.json().catch(() => ({ error: 'Failed to copy profile' }));
				alert(error.error || 'Failed to copy profile');
			} else {
				await invalidateAll();
			}
		} finally {
			delete actionLoading[profileId];
			actionLoading = { ...actionLoading };
		}
	}

	async function handleBuildTools() {
		if (!buildVersion.trim()) {
			buildError = 'Version is required';
			return;
		}
		buildError = '';

		actionLoading.buildtools = true;
		try {
			const res = await fetch('/api/host/profiles/buildtools', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ group: buildGroup, version: buildVersion.trim() })
			});

			if (!res.ok) {
				const error = await res.json().catch(() => ({ error: 'Failed to run BuildTools' }));
				buildError = error.error || 'Failed to run BuildTools';
			} else {
				buildVersion = '';
				await invalidateAll();
			}
		} finally {
			delete actionLoading.buildtools;
			actionLoading = { ...actionLoading };
		}
	}

	async function handleDelete(profileId: string) {
		if (!confirm(`Delete BuildTools profile "${profileId}"?`)) {
			return;
		}

		actionLoading[profileId] = true;
		try {
			const res = await fetch(`/api/host/profiles/buildtools/${profileId}`, {
				method: 'DELETE'
			});
			if (!res.ok) {
				const error = await res.json().catch(() => ({ error: 'Failed to delete profile' }));
				alert(error.error || 'Failed to delete profile');
			} else {
				await invalidateAll();
			}
		} finally {
			delete actionLoading[profileId];
			actionLoading = { ...actionLoading };
		}
	}
</script>

<div class="page-header">
	<div>
		<h1>Profiles</h1>
		<p class="subtitle">Manage Minecraft server jars and BuildTools builds</p>
	</div>
</div>

<div class="buildtools-card">
	<h2>BuildTools</h2>
	<p>Compile Spigot or CraftBukkit jars directly on the host.</p>
	<div class="buildtools-form">
		<label>
			Group
			<select bind:value={buildGroup}>
				<option value="spigot">Spigot</option>
				<option value="craftbukkit">CraftBukkit</option>
			</select>
		</label>
		<label>
			Version
			<input type="text" bind:value={buildVersion} placeholder="1.20.4" />
		</label>
		<button class="btn-primary" onclick={handleBuildTools} disabled={actionLoading.buildtools}>
			{actionLoading.buildtools ? 'Building...' : 'Run BuildTools'}
		</button>
	</div>
	{#if buildError}
		<p class="error">{buildError}</p>
	{/if}
</div>

{#if data.profiles.error}
	<div class="error-box">
		<p>Failed to load profiles: {data.profiles.error}</p>
	</div>
{:else if data.profiles.data && data.profiles.data.length > 0}
	<div class="profiles-grid">
		{#each data.profiles.data as profile}
			<div class="profile-card">
				<div class="card-header">
					<h3>{profile.id}</h3>
					<span class="badge" class:badge-ready={profile.downloaded}>
						{profile.downloaded ? 'Ready' : 'Not downloaded'}
					</span>
				</div>
				<div class="card-body">
					<div class="info-row">
						<span class="label">Group</span>
						<span>{profile.group}</span>
					</div>
					<div class="info-row">
						<span class="label">Version</span>
						<span>{profile.version}</span>
					</div>
					{#if profile.filename}
						<div class="info-row">
							<span class="label">File</span>
							<span class="mono">{profile.filename}</span>
						</div>
					{/if}
				</div>
				<div class="card-actions">
					{#if !profile.downloaded}
						<button
							class="btn-action btn-primary"
							onclick={() => handleDownload(profile.id)}
							disabled={actionLoading[profile.id]}
						>
							Download
						</button>
					{:else}
						<div class="copy-group">
							<select
								value={copyTargets[profile.id] ?? ''}
								onchange={(event) => {
									const value = (event.currentTarget as HTMLSelectElement).value;
									copyTargets[profile.id] = value;
									copyTargets = { ...copyTargets };
								}}
							>
								<option value="">Select server</option>
								{#if data.servers.data}
									{#each data.servers.data as server}
										<option value={server.name}>{server.name}</option>
									{/each}
								{/if}
							</select>
							<button
								class="btn-action"
								onclick={() => handleCopy(profile.id)}
								disabled={actionLoading[profile.id] || !copyTargets[profile.id]}
							>
								Copy to server
							</button>
						</div>
					{/if}
					{#if profile.type === 'buildtools'}
						<button
							class="btn-action btn-danger"
							onclick={() => handleDelete(profile.id)}
							disabled={actionLoading[profile.id]}
						>
							Delete
						</button>
					{/if}
				</div>
			</div>
		{/each}
	</div>
{:else}
	<div class="empty-state">
		<h2>No profiles yet</h2>
		<p>Download a profile or build one with BuildTools.</p>
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

	.buildtools-card {
		background: #1a1e2f;
		border-radius: 16px;
		padding: 20px;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
		margin-bottom: 24px;
	}

	.buildtools-card h2 {
		margin: 0 0 8px;
	}

	.buildtools-card p {
		margin: 0 0 16px;
		color: #9aa2c5;
	}

	.buildtools-form {
		display: grid;
		grid-template-columns: 1fr 1fr auto;
		gap: 12px;
		align-items: end;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 6px;
		font-size: 13px;
		color: #aab2d3;
	}

	input,
	select {
		background: #141827;
		border: 1px solid #2a2f47;
		border-radius: 8px;
		padding: 10px 12px;
		color: #eef0f8;
		font-family: inherit;
		font-size: 14px;
	}

	.btn-primary {
		background: #5865f2;
		color: #fff;
		border: none;
		border-radius: 8px;
		padding: 12px 20px;
		font-size: 14px;
		font-weight: 600;
		cursor: pointer;
	}

	.btn-primary:disabled {
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

	.error {
		color: #ff9f9f;
		margin-top: 8px;
		font-size: 13px;
	}

	.profiles-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
		gap: 20px;
	}

	.profile-card {
		background: #1a1e2f;
		border-radius: 16px;
		padding: 18px;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
		display: flex;
		flex-direction: column;
		gap: 12px;
	}

	.card-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		gap: 12px;
	}

	.card-header h3 {
		margin: 0;
		font-size: 18px;
	}

	.badge {
		padding: 4px 10px;
		border-radius: 999px;
		font-size: 12px;
		background: rgba(255, 159, 159, 0.15);
		color: #ff9f9f;
	}

	.badge-ready {
		background: rgba(122, 230, 141, 0.15);
		color: #7ae68d;
	}

	.card-body {
		display: flex;
		flex-direction: column;
		gap: 6px;
		font-size: 14px;
		color: #d4d9f1;
	}

	.info-row {
		display: flex;
		justify-content: space-between;
		gap: 12px;
	}

	.label {
		color: #9aa2c5;
	}

	.mono {
		font-family: 'Courier New', monospace;
		font-size: 12px;
		color: #c9d1d9;
	}

	.card-actions {
		display: flex;
		flex-wrap: wrap;
		gap: 8px;
		align-items: center;
	}

	.copy-group {
		display: flex;
		gap: 8px;
		flex: 1;
	}

	.copy-group select {
		flex: 1;
	}

	.btn-action {
		background: #2b2f45;
		color: #d4d9f1;
		border: none;
		border-radius: 6px;
		padding: 8px 12px;
		font-size: 13px;
		cursor: pointer;
	}

	.btn-action:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.btn-danger {
		background: rgba(255, 92, 92, 0.15);
		color: #ff9f9f;
	}

	.empty-state {
		text-align: center;
		padding: 60px 20px;
		color: #8e96bb;
	}

	@media (max-width: 720px) {
		.buildtools-form {
			grid-template-columns: 1fr;
		}
		.copy-group {
			flex-direction: column;
		}
	}
</style>
