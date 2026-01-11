namespace MineOS.Domain.Entities;

public class MigrationHistory
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public required string FromVersion { get; set; }
    public required string ToVersion { get; set; }
    public required string Status { get; set; } // "Pending", "InProgress", "Completed", "Failed", "RolledBack"
    public string? BackupPath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset MigratedAt { get; set; }
}
