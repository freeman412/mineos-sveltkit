namespace MineOS.Application.Dtos;

public record InstalledModDto(
    string FileName,
    long SizeBytes,
    DateTimeOffset ModifiedAt,
    bool IsDisabled
);

public record CurseForgeCategoryDto(
    int Id,
    string Name,
    string? Url,
    string? IconUrl
);

public record CurseForgeFileDto(
    int Id,
    string FileName,
    long FileLength,
    DateTimeOffset FileDate,
    string? DownloadUrl,
    IReadOnlyList<string> GameVersions,
    int? ReleaseType
);

public record CurseForgeModSummaryDto(
    int Id,
    string Name,
    string Summary,
    long DownloadCount,
    string? LogoUrl,
    int? LatestFileId,
    IReadOnlyList<CurseForgeCategoryDto> Categories
);

public record CurseForgeModDto(
    int Id,
    string Name,
    string Summary,
    long DownloadCount,
    string? LogoUrl,
    IReadOnlyList<CurseForgeCategoryDto> Categories,
    IReadOnlyList<CurseForgeFileDto> LatestFiles
);

public record CurseForgeSearchResultDto(
    int Index,
    int PageSize,
    int ResultCount,
    int TotalCount,
    IReadOnlyList<CurseForgeModSummaryDto> Results
);
