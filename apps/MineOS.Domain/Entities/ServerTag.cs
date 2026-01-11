namespace MineOS.Domain.Entities;

public class ServerTag
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
