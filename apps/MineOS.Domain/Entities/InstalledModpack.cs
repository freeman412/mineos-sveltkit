namespace MineOS.Domain.Entities;

public sealed class InstalledModpack
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public int CurseForgeProjectId { get; set; }
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? LogoUrl { get; set; }
    public int ModCount { get; set; }
    public DateTimeOffset InstalledAt { get; set; }
    public ICollection<InstalledModRecord> Mods { get; set; } = new List<InstalledModRecord>();
}
