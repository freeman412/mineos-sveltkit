namespace MineOS.Domain.Entities;

public class ServerNote
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Content { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
