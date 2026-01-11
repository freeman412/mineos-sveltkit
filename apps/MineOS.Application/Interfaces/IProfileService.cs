using MineOS.Application.Dtos;

namespace MineOS.Application.Interfaces;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileDto>> ListProfilesAsync(CancellationToken cancellationToken);
    Task<ProfileDto?> GetProfileAsync(string id, CancellationToken cancellationToken);
    Task<string> DownloadProfileAsync(string id, CancellationToken cancellationToken);
    Task CopyProfileToServerAsync(string profileId, string serverName, CancellationToken cancellationToken);
    IAsyncEnumerable<ProfileDownloadProgressDto> StreamDownloadProgressAsync(string id, CancellationToken cancellationToken);
    Task<BuildToolsRunDto> StartBuildToolsAsync(string group, string version, CancellationToken cancellationToken);
    Task<IReadOnlyList<BuildToolsRunDto>> ListBuildToolsRunsAsync(CancellationToken cancellationToken);
    Task<BuildToolsRunDto?> GetBuildToolsRunAsync(string runId, CancellationToken cancellationToken);
    IAsyncEnumerable<BuildToolsLogEntryDto> StreamBuildToolsLogAsync(string runId, CancellationToken cancellationToken);
    Task DeleteBuildToolsAsync(string id, CancellationToken cancellationToken);
}
