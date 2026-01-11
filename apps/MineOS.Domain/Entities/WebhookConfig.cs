namespace MineOS.Domain.Entities;

public class WebhookConfig
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Type { get; set; } // "Discord", "Slack", "Custom"
    public required string Url { get; set; }
    public required string Events { get; set; } // JSON array: ["server_start", "server_stop", "player_join", "player_leave", "crash"]
    public bool Enabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
