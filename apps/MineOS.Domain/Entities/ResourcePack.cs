namespace MineOS.Domain.Entities;

public class ResourcePack
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public string? Hash { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
