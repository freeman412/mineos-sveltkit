using MineOS.Application.Dtos;

namespace MineOS.Application.Interfaces;

public interface ICurseForgeService
{
    Task<CurseForgeSearchResultDto> SearchModsAsync(
        string query,
        int? classId,
        int index,
        int pageSize,
        string? sort,
        string? order,
        long? minDownloads,
        CancellationToken cancellationToken);

    Task<CurseForgeModDto> GetModAsync(int modId, CancellationToken cancellationToken);

    Task<CurseForgeFileDto> GetModFileAsync(int modId, int fileId, CancellationToken cancellationToken);

    Task<string> GetModFileDownloadUrlAsync(int modId, int fileId, CancellationToken cancellationToken);
}
