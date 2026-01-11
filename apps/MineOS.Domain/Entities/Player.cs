namespace MineOS.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Uuid { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? LastSeen { get; set; }
    public long PlayTimeSeconds { get; set; }
    public bool Banned { get; set; }
    public bool Whitelisted { get; set; }
    public bool IsOp { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
