namespace MineOS.Domain.Entities;

public class ServerTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Config { get; set; } // JSON with server.config and server.properties
    public string? Description { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
