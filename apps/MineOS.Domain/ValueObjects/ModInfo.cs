namespace MineOS.Domain.ValueObjects;

public record ModInfo
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public int? LatestFileId { get; init; }
}
