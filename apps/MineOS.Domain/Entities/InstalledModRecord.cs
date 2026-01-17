namespace MineOS.Domain.Entities;

public sealed class InstalledModRecord
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string FileName { get; set; }
    public int? CurseForgeProjectId { get; set; }
    public string? ModName { get; set; }
    public int? ModpackId { get; set; }
    public InstalledModpack? Modpack { get; set; }
    public DateTimeOffset InstalledAt { get; set; }
}
