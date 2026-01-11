namespace MineOS.Domain.Entities;

public class PlayerBan
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public required string Reason { get; set; }
    public required string BannedBy { get; set; }
    public DateTimeOffset BannedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool Active { get; set; }

    // Navigation property
    public Player? Player { get; set; }
}
