namespace MineOS.Domain.Entities;

public class Alert
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Type { get; set; } // "LowTps", "HighMemory", "ServerCrash", "PlayerCount"
    public required string Threshold { get; set; } // JSON with threshold config
    public bool Enabled { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastTriggeredAt { get; set; }
}
