package commands

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"reflect"
	"strings"
	"testing"
	"time"

	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/domain/config"
	"github.com/freemancraft/mineos-sveltekit/tools/mineos-cli/internal/domain/ports"
)

func TestWarningMilestones(t *testing.T) {
	cases := []struct {
		total int
		want  []int
	}{
		{60, []int{60, 30, 10, 5}},
		{45, []int{45, 30, 10, 5}},
		{30, []int{30, 10, 5}},
		{15, []int{15, 10, 5}},
		{10, []int{10, 5}},
		{5, []int{5}},
		{3, []int{3}},
		{1, []int{1}},
		{0, nil},
		{-5, nil},
	}
	for _, tc := range cases {
		got := warningMilestones(tc.total)
		if !reflect.DeepEqual(got, tc.want) {
			t.Errorf("warningMilestones(%d) = %v, want %v", tc.total, got, tc.want)
		}
	}
}

func TestEffectiveRestartWarning(t *testing.T) {
	cases := []struct {
		name     string
		cfg      config.Config
		override int
		want     int
	}{
		{"flag wins over env", config.Config{RestartWarningSeconds: "60"}, 15, 15},
		{"flag of zero disables", config.Config{RestartWarningSeconds: "60"}, 0, 0},
		{"negative flag means unset, falls back to env", config.Config{RestartWarningSeconds: "45"}, -1, 45},
		{"env of zero disables", config.Config{RestartWarningSeconds: "0"}, -1, 0},
		{"empty env falls back to default", config.Config{}, -1, defaultRestartWarning},
		{"unparseable env falls back to default", config.Config{RestartWarningSeconds: "soon"}, -1, defaultRestartWarning},
		{"negative env falls back to default", config.Config{RestartWarningSeconds: "-9"}, -1, defaultRestartWarning},
	}
	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			if got := effectiveRestartWarning(tc.cfg, tc.override); got != tc.want {
				t.Errorf("got %d, want %d", got, tc.want)
			}
		})
	}
}

// sentCommand records one broadcast so tests can assert on real ordering.
type sentCommand struct {
	server  string
	command string
}

type fakeBroadcaster struct {
	sent   []sentCommand
	slept  []time.Duration
	failOn string
	cancel context.CancelFunc
	// cancelAfter cancels the context once this many sleeps have happened.
	cancelAfter int
}

func (f *fakeBroadcaster) send(_ context.Context, name, command string) error {
	f.sent = append(f.sent, sentCommand{server: name, command: command})
	if f.failOn != "" && name == f.failOn {
		return errors.New("server is not running")
	}
	return nil
}

func (f *fakeBroadcaster) sleep(ctx context.Context, d time.Duration) error {
	f.slept = append(f.slept, d)
	if f.cancel != nil && len(f.slept) >= f.cancelAfter {
		f.cancel()
	}
	if err := ctx.Err(); err != nil {
		return err
	}
	return nil
}

func (f *fakeBroadcaster) totalSlept() time.Duration {
	var total time.Duration
	for _, d := range f.slept {
		total += d
	}
	return total
}

func players(n int) *int { return &n }

func newWarner(t *testing.T, servers []ports.Server, f *fakeBroadcaster, out *bytes.Buffer) restartWarner {
	t.Helper()
	return restartWarner{servers: servers, send: f.send, sleep: f.sleep, out: out}
}

func TestRestartWarnerBroadcastsTieredCountdownToEveryRunningServer(t *testing.T) {
	servers := []ports.Server{
		{Name: "survival", Up: true, PlayersOnline: players(2)},
		{Name: "creative", Up: true, PlayersOnline: players(0)},
		{Name: "archived", Up: false, PlayersOnline: players(0)},
	}
	f := &fakeBroadcaster{}
	out := &bytes.Buffer{}

	if err := newWarner(t, servers, f, out).run(context.Background(), 60); err != nil {
		t.Fatalf("run: %v", err)
	}

	want := []sentCommand{}
	for _, secs := range []int{60, 30, 10, 5} {
		msg := fmt.Sprintf("say [MineOS] Server restarting in %d seconds", secs)
		want = append(want, sentCommand{"survival", msg}, sentCommand{"creative", msg})
	}
	want = append(want,
		sentCommand{"survival", "say [MineOS] Restarting now..."},
		sentCommand{"creative", "say [MineOS] Restarting now..."},
	)

	if !reflect.DeepEqual(f.sent, want) {
		t.Errorf("broadcast mismatch\ngot:  %v\nwant: %v", f.sent, want)
	}
	if got := f.totalSlept(); got != 60*time.Second {
		t.Errorf("total sleep = %v, want 60s", got)
	}
}

func TestRestartWarnerUsesSingularSecondForFinalTick(t *testing.T) {
	servers := []ports.Server{{Name: "survival", Up: true, PlayersOnline: players(1)}}
	f := &fakeBroadcaster{}

	if err := newWarner(t, servers, f, &bytes.Buffer{}).run(context.Background(), 1); err != nil {
		t.Fatalf("run: %v", err)
	}

	if f.sent[0].command != "say [MineOS] Server restarting in 1 second" {
		t.Errorf("got %q, want singular phrasing", f.sent[0].command)
	}
}

func TestRestartWarnerSkipsEntirelyWhenNobodyIsOnline(t *testing.T) {
	servers := []ports.Server{
		{Name: "survival", Up: true, PlayersOnline: players(0)},
		{Name: "creative", Up: true, PlayersOnline: nil},
	}
	f := &fakeBroadcaster{}
	out := &bytes.Buffer{}

	if err := newWarner(t, servers, f, out).run(context.Background(), 60); err != nil {
		t.Fatalf("run: %v", err)
	}

	if len(f.sent) != 0 {
		t.Errorf("expected no broadcasts, got %v", f.sent)
	}
	if f.totalSlept() != 0 {
		t.Errorf("expected no delay, slept %v", f.totalSlept())
	}
	if !strings.Contains(out.String(), "No players online") {
		t.Errorf("expected an explanation on stdout, got %q", out.String())
	}
}

func TestRestartWarnerSkipsWhenNoServersAreRunning(t *testing.T) {
	servers := []ports.Server{{Name: "archived", Up: false, PlayersOnline: players(0)}}
	f := &fakeBroadcaster{}

	if err := newWarner(t, servers, f, &bytes.Buffer{}).run(context.Background(), 60); err != nil {
		t.Fatalf("run: %v", err)
	}
	if len(f.sent) != 0 || f.totalSlept() != 0 {
		t.Errorf("expected no work, sent %v slept %v", f.sent, f.totalSlept())
	}
}

func TestRestartWarnerDisabledWhenWarnSecondsIsZero(t *testing.T) {
	servers := []ports.Server{{Name: "survival", Up: true, PlayersOnline: players(5)}}
	f := &fakeBroadcaster{}

	if err := newWarner(t, servers, f, &bytes.Buffer{}).run(context.Background(), 0); err != nil {
		t.Fatalf("run: %v", err)
	}
	if len(f.sent) != 0 || f.totalSlept() != 0 {
		t.Errorf("expected no work, sent %v slept %v", f.sent, f.totalSlept())
	}
}

func TestRestartWarnerKeepsGoingWhenOneServerRejectsTheCommand(t *testing.T) {
	servers := []ports.Server{
		{Name: "broken", Up: true, PlayersOnline: players(1)},
		{Name: "survival", Up: true, PlayersOnline: players(3)},
	}
	f := &fakeBroadcaster{failOn: "broken"}
	out := &bytes.Buffer{}

	if err := newWarner(t, servers, f, out).run(context.Background(), 5); err != nil {
		t.Fatalf("run should not fail because one server errored: %v", err)
	}

	// 5s warning + final notice, both servers attempted each time.
	if len(f.sent) != 4 {
		t.Errorf("expected 4 broadcast attempts, got %d: %v", len(f.sent), f.sent)
	}
	if !strings.Contains(out.String(), "broken") {
		t.Errorf("expected the failing server named on stdout, got %q", out.String())
	}
}

func TestRestartWarnerAbortsWhenContextIsCancelled(t *testing.T) {
	servers := []ports.Server{{Name: "survival", Up: true, PlayersOnline: players(1)}}
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	f := &fakeBroadcaster{cancel: cancel, cancelAfter: 1}

	err := newWarner(t, servers, f, &bytes.Buffer{}).run(ctx, 60)

	if !errors.Is(err, context.Canceled) {
		t.Fatalf("expected context.Canceled, got %v", err)
	}
	// Only the first (60s) warning should have gone out before the cancel.
	if len(f.sent) != 1 {
		t.Errorf("expected 1 broadcast before cancel, got %d: %v", len(f.sent), f.sent)
	}
}
