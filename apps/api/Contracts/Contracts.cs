namespace MineOS.Api.Contracts;

public record DiskMetrics(long AvailableBytes, long FreeBytes, long TotalBytes);

public record HostMetrics(long UptimeSeconds, long FreeMemBytes, double[] LoadAvg, DiskMetrics Disk);

public record ServerSummary(
    string Name,
    bool Up,
    string? Profile,
    int? Port,
    int? PlayersOnline,
    int? PlayersMax);

public record Profile(
    string Id,
    string Group,
    string Type,
    string Version,
    string ReleaseTime,
    string Url,
    string Filename,
    bool Downloaded,
    object? Progress);

public record ArchiveEntry(DateTimeOffset Time, long Size, string Filename);

public record IncrementEntry(DateTimeOffset Time, string Step, long? Size, long? CumulativeSize);

public record CronJob(string Hash, string Source, string Action, string? Msg, bool Enabled);

public record Notice(
    string Uuid,
    string Command,
    bool Success,
    string? Err,
    long TimeInitiated,
    long TimeResolved);

public record MemoryInfo(long? RssBytes);

public record PingInfo(string? ServerVersion, string? Motd, int? PlayersOnline, int? PlayersMax);

public record QueryInfo(Dictionary<string, object>? Raw);

public record ServerHeartbeat(
    bool Up,
    MemoryInfo? Memory,
    PingInfo? Ping,
    QueryInfo? Query,
    long Timestamp);

public record ConsoleCommand(string Command);

public record CreateServerRequest(string ServerName, Dictionary<string, string>? Properties, bool Unconventional);

public record DeleteServerRequest(bool DeleteLive, bool DeleteBackups, bool DeleteArchives);

public record ActionRequest(string? Step, int? Niceness);

public record CreateCronRequest(string Source, string Action, string? Msg);

public record UpdateCronRequest(bool Enabled);
