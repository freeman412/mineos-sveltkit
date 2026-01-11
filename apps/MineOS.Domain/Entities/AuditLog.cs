namespace MineOS.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Action { get; set; } // "server_start", "server_stop", "backup_create", "file_delete", etc.
    public string? ServerName { get; set; }
    public string? Details { get; set; } // JSON with additional context
    public DateTimeOffset Timestamp { get; set; }
}
