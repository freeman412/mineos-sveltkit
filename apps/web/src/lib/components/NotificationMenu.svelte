<script lang="ts">
	import type { SystemNotification } from '$lib/api/types';
	import { onMount } from 'svelte';
	import { modal } from '$lib/stores/modal';

	let notifications = $state<SystemNotification[]>([]);
	let isOpen = $state(false);
	let loading = $state(false);

	const unreadCount = $derived(notifications.filter((n) => !n.isRead && !n.dismissedAt).length);

	onMount(() => {
		loadNotifications();
		// Poll for new notifications every 30 seconds
		const interval = setInterval(loadNotifications, 30000);
		return () => clearInterval(interval);
	});

	async function loadNotifications() {
		if (loading) return;
		loading = true;
		try {
			const res = await fetch('/api/notifications?includeDismissed=false');
			if (res.ok) {
				notifications = await res.json();
			}
		} catch (err) {
			console.error('Failed to load notifications:', err);
		} finally {
			loading = false;
		}
	}

	async function markAsRead(id: number) {
		try {
			const res = await fetch(`/api/notifications/${id}/read`, { method: 'PATCH' });
			if (res.ok) {
				const notification = notifications.find((n) => n.id === id);
				if (notification) {
					notification.isRead = true;
				}
			}
		} catch (err) {
			console.error('Failed to mark as read:', err);
		}
	}

	async function dismiss(id: number) {
		try {
			const res = await fetch(`/api/notifications/${id}/dismiss`, { method: 'PATCH' });
			if (res.ok) {
				notifications = notifications.filter((n) => n.id !== id);
			}
		} catch (err) {
			console.error('Failed to dismiss:', err);
		}
	}

	async function deleteNotification(id: number) {
		const confirmed = await modal.confirm('Delete this notification?', 'Delete Notification');
		if (!confirmed) return;

		try {
			const res = await fetch(`/api/notifications/${id}`, { method: 'DELETE' });
			if (res.ok) {
				notifications = notifications.filter((n) => n.id !== id);
			}
		} catch (err) {
			await modal.error(err instanceof Error ? err.message : 'Delete failed');
		}
	}

	async function dismissAll() {
		const ids = notifications.filter((n) => !n.dismissedAt).map((n) => n.id);
		if (ids.length === 0) return;

		try {
			const res = await fetch('/api/notifications/dismiss', {
				method: 'PATCH',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(ids)
			});
			if (res.ok) {
				notifications = [];
			}
		} catch (err) {
			console.error('Failed to dismiss all:', err);
		}
	}

	function getIconForType(type: SystemNotification['type']) {
		switch (type) {
			case 'info':
				return 'ℹ️';
			case 'warning':
				return '⚠️';
			case 'error':
				return '❌';
			case 'success':
				return '✅';
			default:
				return '📢';
		}
	}

	function getColorForType(type: SystemNotification['type']) {
		switch (type) {
			case 'info':
				return '#5b9eff';
			case 'warning':
				return '#ffb74d';
			case 'error':
				return '#ff6b6b';
			case 'success':
				return '#6ab04c';
			default:
				return '#9aa2c5';
		}
	}

	function formatTime(dateString: string) {
		const date = new Date(dateString);
		const now = new Date();
		const diff = now.getTime() - date.getTime();
		const minutes = Math.floor(diff / 60000);
		const hours = Math.floor(minutes / 60);
		const days = Math.floor(hours / 24);

		if (minutes < 1) return 'just now';
		if (minutes < 60) return `${minutes}m ago`;
		if (hours < 24) return `${hours}h ago`;
		if (days < 7) return `${days}d ago`;
		return date.toLocaleDateString();
	}

	function toggleMenu() {
		isOpen = !isOpen;
		if (isOpen) {
			loadNotifications();
		}
	}

	function handleClickOutside(event: MouseEvent) {
		const target = event.target as HTMLElement;
		if (!target.closest('.notification-menu')) {
			isOpen = false;
		}
	}
</script>

<svelte:window onclick={handleClickOutside} />

<div class="notification-menu">
	<button class="notification-bell" onclick={toggleMenu} aria-label="Notifications">
		<span class="bell-icon">🔔</span>
		{#if unreadCount > 0}
			<span class="badge">{unreadCount > 9 ? '9+' : unreadCount}</span>
		{/if}
	</button>

	{#if isOpen}
		<div class="notification-dropdown">
			<div class="dropdown-header">
				<h3>Notifications</h3>
				{#if notifications.length > 0}
					<button class="dismiss-all-btn" onclick={dismissAll}>Dismiss All</button>
				{/if}
			</div>

			<div class="notification-list">
				{#if notifications.length === 0}
					<div class="empty-state">
						<p>No notifications</p>
					</div>
				{:else}
					{#each notifications as notification (notification.id)}
						<div
							class="notification-item"
							class:unread={!notification.isRead}
							onclick={() => !notification.isRead && markAsRead(notification.id)}
						>
							<div class="notification-icon" style="color: {getColorForType(notification.type)}">
								{getIconForType(notification.type)}
							</div>
							<div class="notification-content">
								<div class="notification-header">
									<h4>{notification.title}</h4>
									<span class="time">{formatTime(notification.createdAt)}</span>
								</div>
								<p>{notification.message}</p>
								{#if notification.serverName}
									<span class="server-tag">{notification.serverName}</span>
								{/if}
							</div>
							<div class="notification-actions">
								<button
									class="action-btn"
									onclick={(e) => {
										e.stopPropagation();
										dismiss(notification.id);
									}}
									title="Dismiss"
								>
									✓
								</button>
								<button
									class="action-btn danger"
									onclick={(e) => {
										e.stopPropagation();
										deleteNotification(notification.id);
									}}
									title="Delete"
								>
									🗑️
								</button>
							</div>
						</div>
					{/each}
				{/if}
			</div>
		</div>
	{/if}
</div>

<style>
	.notification-menu {
		position: relative;
	}

	.notification-bell {
		position: relative;
		background: none;
		border: none;
		color: #eef0f8;
		font-size: 20px;
		cursor: pointer;
		padding: 8px;
		border-radius: 8px;
		transition: background 0.2s;
	}

	.notification-bell:hover {
		background: rgba(255, 255, 255, 0.1);
	}

	.bell-icon {
		display: block;
	}

	.badge {
		position: absolute;
		top: 4px;
		right: 4px;
		background: #ff6b6b;
		color: white;
		font-size: 10px;
		font-weight: 600;
		padding: 2px 5px;
		border-radius: 10px;
		min-width: 18px;
		text-align: center;
	}

	.notification-dropdown {
		position: absolute;
		top: calc(100% + 8px);
		right: 0;
		width: 400px;
		max-height: 500px;
		background: #141827;
		border-radius: 12px;
		border: 1px solid #2a2f47;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
		overflow: hidden;
		z-index: 1000;
	}

	.dropdown-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 16px 20px;
		border-bottom: 1px solid #2a2f47;
	}

	.dropdown-header h3 {
		margin: 0;
		font-size: 18px;
		font-weight: 600;
		color: #eef0f8;
	}

	.dismiss-all-btn {
		background: none;
		border: none;
		color: #9aa2c5;
		font-size: 13px;
		cursor: pointer;
		padding: 4px 8px;
		border-radius: 6px;
		transition: background 0.2s;
	}

	.dismiss-all-btn:hover {
		background: rgba(255, 255, 255, 0.05);
		color: #eef0f8;
	}

	.notification-list {
		max-height: 420px;
		overflow-y: auto;
	}

	.empty-state {
		padding: 40px 20px;
		text-align: center;
		color: #8890b1;
	}

	.empty-state p {
		margin: 0;
	}

	.notification-item {
		display: flex;
		gap: 12px;
		padding: 16px 20px;
		border-bottom: 1px solid #2a2f47;
		cursor: pointer;
		transition: background 0.2s;
	}

	.notification-item:hover {
		background: #1a1f33;
	}

	.notification-item.unread {
		background: rgba(88, 101, 242, 0.05);
	}

	.notification-icon {
		font-size: 20px;
		flex-shrink: 0;
	}

	.notification-content {
		flex: 1;
		min-width: 0;
	}

	.notification-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		gap: 8px;
		margin-bottom: 4px;
	}

	.notification-header h4 {
		margin: 0;
		font-size: 14px;
		font-weight: 600;
		color: #eef0f8;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.time {
		font-size: 11px;
		color: #8890b1;
		flex-shrink: 0;
	}

	.notification-content p {
		margin: 0 0 8px;
		font-size: 13px;
		color: #9aa2c5;
		line-height: 1.4;
	}

	.server-tag {
		display: inline-block;
		background: #1a1f33;
		border: 1px solid #2a2f47;
		padding: 2px 8px;
		border-radius: 4px;
		font-size: 11px;
		color: #9aa2c5;
	}

	.notification-actions {
		display: flex;
		gap: 4px;
		flex-shrink: 0;
	}

	.action-btn {
		background: none;
		border: none;
		color: #9aa2c5;
		font-size: 14px;
		cursor: pointer;
		padding: 4px 8px;
		border-radius: 6px;
		transition: all 0.2s;
	}

	.action-btn:hover {
		background: rgba(255, 255, 255, 0.05);
		color: #eef0f8;
	}

	.action-btn.danger:hover {
		background: rgba(255, 92, 92, 0.1);
		color: #ff9f9f;
	}
</style>
