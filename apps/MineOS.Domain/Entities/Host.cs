namespace MineOS.Domain.Entities;

public class Host
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Hostname { get; set; }
    public int Port { get; set; }
    public required string ApiKey { get; set; }
    public required string Status { get; set; } // "Online", "Offline", "Unreachable"
    public DateTimeOffset? LastPing { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
