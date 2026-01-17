using MineOS.Application.Dtos;

namespace MineOS.Application.Interfaces;

/// <summary>
/// Service for looking up Minecraft player profiles from Mojang API.
/// </summary>
public interface IMojangApiService
{
    /// <summary>
    /// Look up a player profile by username.
    /// </summary>
    /// <param name="username">The Minecraft username to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The player profile if found, null otherwise.</returns>
    Task<MojangProfileDto?> LookupByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Look up a player profile by UUID.
    /// </summary>
    /// <param name="uuid">The player UUID to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The player profile if found, null otherwise.</returns>
    Task<MojangProfileDto?> LookupByUuidAsync(string uuid, CancellationToken cancellationToken);
}
