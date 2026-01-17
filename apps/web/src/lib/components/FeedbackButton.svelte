<script lang="ts">
	let isOpen = $state(false);

	const repoUrl = 'https://github.com/freeman412/mineos-sveltekit';

	function handleClick(type: 'bug' | 'feature' | 'question') {
		const templates: Record<string, string> = {
			bug: 'bug_report.yml',
			feature: 'feature_request.yml',
			question: 'question.yml'
		};
		window.open(`${repoUrl}/issues/new?template=${templates[type]}`, '_blank');
		isOpen = false;
	}
</script>

<div class="feedback-container">
	{#if isOpen}
		<div class="feedback-menu">
			<button class="feedback-option" onclick={() => handleClick('bug')}>
				<span class="option-icon">🐛</span>
				<span class="option-text">Report a Bug</span>
			</button>
			<button class="feedback-option" onclick={() => handleClick('feature')}>
				<span class="option-icon">💡</span>
				<span class="option-text">Request Feature</span>
			</button>
			<button class="feedback-option" onclick={() => handleClick('question')}>
				<span class="option-icon">❓</span>
				<span class="option-text">Ask a Question</span>
			</button>
		</div>
	{/if}

	<button
		class="feedback-button"
		class:open={isOpen}
		onclick={() => (isOpen = !isOpen)}
		title="Send Feedback"
	>
		{#if isOpen}
			<svg viewBox="0 0 24 24" width="24" height="24">
				<path fill="currentColor" d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
			</svg>
		{:else}
			<svg viewBox="0 0 24 24" width="24" height="24">
				<path fill="currentColor" d="M20 2H4c-1.1 0-1.99.9-1.99 2L2 22l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-7 12h-2v-2h2v2zm0-4h-2V6h2v4z"/>
			</svg>
		{/if}
	</button>
</div>

<style>
	.feedback-container {
		position: fixed;
		bottom: 24px;
		right: 24px;
		z-index: 1000;
		display: flex;
		flex-direction: column;
		align-items: flex-end;
		gap: 12px;
	}

	.feedback-menu {
		display: flex;
		flex-direction: column;
		gap: 8px;
		background: #1a1e2f;
		border: 1px solid #2a2f47;
		border-radius: 12px;
		padding: 8px;
		box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
		animation: slideUp 0.2s ease-out;
	}

	@keyframes slideUp {
		from {
			opacity: 0;
			transform: translateY(10px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}

	.feedback-option {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 12px 16px;
		background: transparent;
		border: none;
		border-radius: 8px;
		color: #eef0f8;
		font-size: 14px;
		cursor: pointer;
		transition: background 0.15s;
		white-space: nowrap;
	}

	.feedback-option:hover {
		background: rgba(106, 176, 76, 0.15);
	}

	.option-icon {
		font-size: 18px;
	}

	.option-text {
		font-weight: 500;
	}

	.feedback-button {
		width: 56px;
		height: 56px;
		border-radius: 50%;
		background: linear-gradient(135deg, #6ab04c 0%, #4a8035 100%);
		border: none;
		color: white;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		box-shadow: 0 4px 16px rgba(106, 176, 76, 0.4);
		transition: all 0.2s;
	}

	.feedback-button:hover {
		transform: scale(1.05);
		box-shadow: 0 6px 24px rgba(106, 176, 76, 0.5);
	}

	.feedback-button.open {
		background: linear-gradient(135deg, #ff6b6b 0%, #ee5a5a 100%);
		box-shadow: 0 4px 16px rgba(255, 107, 107, 0.4);
	}

	.feedback-button.open:hover {
		box-shadow: 0 6px 24px rgba(255, 107, 107, 0.5);
	}

	@media (max-width: 768px) {
		.feedback-container {
			bottom: 16px;
			right: 16px;
		}

		.feedback-button {
			width: 48px;
			height: 48px;
		}
	}
</style>
