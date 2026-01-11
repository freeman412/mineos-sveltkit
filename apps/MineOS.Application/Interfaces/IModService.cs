using MineOS.Application.Dtos;

namespace MineOS.Application.Interfaces;

public interface IModService
{
    Task<IReadOnlyList<InstalledModDto>> ListModsAsync(string serverName, CancellationToken cancellationToken);
    Task SaveModAsync(string serverName, string fileName, Stream content, CancellationToken cancellationToken);
    Task DeleteModAsync(string serverName, string fileName, CancellationToken cancellationToken);
    Task<string> GetModPathAsync(string serverName, string fileName, CancellationToken cancellationToken);
    Task InstallModFromCurseForgeAsync(
        string serverName,
        int modId,
        int? fileId,
        IProgress<JobProgressDto> progress,
        CancellationToken cancellationToken);
    Task InstallModpackAsync(
        string serverName,
        int modpackId,
        int? fileId,
        IProgress<JobProgressDto> progress,
        CancellationToken cancellationToken);
}
