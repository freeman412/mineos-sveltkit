namespace MineOS.Domain.Entities;

public sealed class JobRecord
{
    public string JobId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string Status { get; set; } = "queued";
    public int Percentage { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
