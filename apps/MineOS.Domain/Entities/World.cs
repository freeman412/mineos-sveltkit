namespace MineOS.Domain.Entities;

public class World
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Name { get; set; } // "world", "world_nether", "world_the_end"
    public string? Seed { get; set; }
    public string? Type { get; set; } // "normal", "flat", "large_biomes", "amplified"
    public long SizeBytes { get; set; }
    public DateTimeOffset? LastBackup { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
