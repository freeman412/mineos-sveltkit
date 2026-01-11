using MineOS.Application.Dtos;

namespace MineOS.Application.Interfaces;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileDto>> ListProfilesAsync(CancellationToken cancellationToken);
    Task<ProfileDto?> GetProfileAsync(string id, CancellationToken cancellationToken);
    Task<string> DownloadProfileAsync(string id, CancellationToken cancellationToken);
    Task CopyProfileToServerAsync(string profileId, string serverName, CancellationToken cancellationToken);
    IAsyncEnumerable<ProfileDownloadProgressDto> StreamDownloadProgressAsync(string id, CancellationToken cancellationToken);
    Task<ProfileDto> BuildToolsAsync(string group, string version, CancellationToken cancellationToken);
    Task DeleteBuildToolsAsync(string id, CancellationToken cancellationToken);
}
