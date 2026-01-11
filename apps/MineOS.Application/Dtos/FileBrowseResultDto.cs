namespace MineOS.Application.Dtos;

public record FileContentDto(string Path, string Content, long Size, DateTimeOffset Modified);

public record FileBrowseResultDto(
    string Path,
    string Kind,
    IReadOnlyList<FileEntryDto>? Entries,
    FileContentDto? File);
