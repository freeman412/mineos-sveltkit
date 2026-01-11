namespace MineOS.Domain.Entities;

public class PerformanceMetric
{
    public int Id { get; set; }
    public required string ServerName { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public double CpuPercent { get; set; }
    public long RamUsedMb { get; set; }
    public long RamTotalMb { get; set; }
    public double? Tps { get; set; }
    public int PlayerCount { get; set; }
}
