package commands

import (
	"context"
	"fmt"
	"io"
	"strconv"
	"strings"
	"time"

	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/application/usecases"
	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/domain/config"
	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/domain/ports"
	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/infrastructure/api"
)

const defaultRestartWarning = 60

// warningTiers are the countdown points players hear, longest first.
var warningTiers = []int{60, 30, 10, 5}

// warningMilestones returns the countdown points for a warning of total
// seconds, longest first. The total itself is always announced; the standard
// tiers below it fill in the rest.
func warningMilestones(total int) []int {
	if total <= 0 {
		return nil
	}
	milestones := []int{total}
	for _, tier := range warningTiers {
		if tier < total {
			milestones = append(milestones, tier)
		}
	}
	return milestones
}

// effectiveRestartWarning resolves the warning length from the --warn-seconds
// flag, then .env, then the default. A negative override means "flag not set";
// an override of 0 explicitly disables the warning.
func effectiveRestartWarning(cfg config.Config, override int) int {
	if override >= 0 {
		return override
	}
	if value := strings.TrimSpace(cfg.RestartWarningSeconds); value != "" {
		if parsed, err := strconv.Atoi(value); err == nil && parsed >= 0 {
			return parsed
		}
	}
	return defaultRestartWarning
}

// restartWarner broadcasts a countdown to players before a graceful stop.
// send and sleep are injected so the countdown can be tested without a
// live API or real time passing.
type restartWarner struct {
	servers []ports.Server
	send    func(ctx context.Context, name, command string) error
	sleep   func(ctx context.Context, d time.Duration) error
	out     io.Writer
}

func (w restartWarner) run(ctx context.Context, warnSeconds int) error {
	milestones := warningMilestones(warnSeconds)
	if len(milestones) == 0 {
		return nil
	}

	var running []string
	anyPlayers := false
	for _, server := range w.servers {
		if !server.Up {
			continue
		}
		running = append(running, server.Name)
		if server.PlayersOnline != nil && *server.PlayersOnline > 0 {
			anyPlayers = true
		}
	}

	if len(running) == 0 {
		return nil
	}
	if !anyPlayers {
		fmt.Fprintln(w.out, "No players online; skipping restart warning.")
		return nil
	}

	fmt.Fprintf(w.out, "Warning players: restart in %d seconds...\n", warnSeconds)

	for i, milestone := range milestones {
		w.broadcast(ctx, running, fmt.Sprintf("say [MineOS] Server restarting in %s", pluralSeconds(milestone)))
		// Wait until the next milestone, or out the remainder after the last one.
		next := 0
		if i+1 < len(milestones) {
			next = milestones[i+1]
		}
		if err := w.sleep(ctx, time.Duration(milestone-next)*time.Second); err != nil {
			return err
		}
	}

	w.broadcast(ctx, running, "say [MineOS] Restarting now...")
	return nil
}

func (w restartWarner) broadcast(ctx context.Context, servers []string, command string) {
	for _, name := range servers {
		if err := w.send(ctx, name, command); err != nil {
			fmt.Fprintf(w.out, "Warning: could not warn %s: %v\n", name, err)
		}
	}
}

func pluralSeconds(n int) string {
	if n == 1 {
		return "1 second"
	}
	return fmt.Sprintf("%d seconds", n)
}

// sleepWithContext waits for d, returning early if the command is cancelled.
func sleepWithContext(ctx context.Context, d time.Duration) error {
	if d <= 0 {
		return ctx.Err()
	}
	timer := time.NewTimer(d)
	defer timer.Stop()
	select {
	case <-ctx.Done():
		return ctx.Err()
	case <-timer.C:
		return nil
	}
}

// warnPlayersOfRestart announces an impending restart to players on every
// running server, then waits out the countdown. Failures are reported but
// never block the restart itself.
func warnPlayersOfRestart(ctx context.Context, loadConfig *usecases.LoadConfigUseCase, out io.Writer, warnSeconds int) error {
	if warnSeconds <= 0 {
		return nil
	}

	_, err := withApiKeyRetry(ctx, loadConfig, out, func(_ config.Config, client *api.Client) error {
		servers, err := client.ListServers(ctx)
		if err != nil {
			return err
		}
		return restartWarner{
			servers: servers,
			send:    client.SendConsoleCommand,
			sleep:   sleepWithContext,
			out:     out,
		}.run(ctx, warnSeconds)
	})
	return err
}
