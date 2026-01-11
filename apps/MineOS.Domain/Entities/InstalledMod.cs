namespace MineOS.Domain.Entities;

public sealed class InstalledMod
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public bool IsDisabled { get; set; }
    public string? Source { get; set; }
}
