namespace MineOS.Domain.Entities;

public class Plugin
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Name { get; set; }
    public string? Version { get; set; }
    public bool Enabled { get; set; }
    public string? ConfigPath { get; set; }
    public DateTimeOffset InstalledAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
