using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MineOS.Application.Dtos;
using MineOS.Application.Interfaces;

namespace MineOS.Infrastructure.Services;

/// <summary>
/// Service for looking up Minecraft player profiles from Mojang API.
/// Uses mc-heads.net for avatar URLs as it's more reliable and has CORS support.
/// </summary>
public sealed class MojangApiService : IMojangApiService
{
    private const string MojangApiBase = "https://api.mojang.com";
    private const string AvatarUrlTemplate = "https://mc-heads.net/avatar/{0}/64";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MojangApiService> _logger;

    public MojangApiService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<MojangApiService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MojangProfileDto?> LookupByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var cacheKey = $"mojang:username:{username.ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out MojangProfileDto? cached))
        {
            return cached;
        }

        try
        {
            var url = $"{MojangApiBase}/users/profiles/minecraft/{Uri.EscapeDataString(username)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Player {Username} not found in Mojang API", username);
                    return null;
                }

                _logger.LogWarning(
                    "Mojang API returned {StatusCode} for username lookup: {Username}",
                    response.StatusCode,
                    username);
                return null;
            }

            var mojangResponse = await response.Content.ReadFromJsonAsync<MojangUsernameResponse>(
                JsonOptions,
                cancellationToken);

            if (mojangResponse == null || string.IsNullOrEmpty(mojangResponse.Id))
            {
                return null;
            }

            var formattedUuid = FormatUuid(mojangResponse.Id);
            var profile = new MojangProfileDto(
                formattedUuid,
                mojangResponse.Name,
                string.Format(AvatarUrlTemplate, formattedUuid));

            _cache.Set(cacheKey, profile, CacheDuration);
            _cache.Set($"mojang:uuid:{formattedUuid.ToLowerInvariant()}", profile, CacheDuration);

            return profile;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to lookup username {Username} from Mojang API", username);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Mojang API response for username {Username}", username);
            return null;
        }
    }

    public async Task<MojangProfileDto?> LookupByUuidAsync(string uuid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uuid))
        {
            return null;
        }

        var normalizedUuid = uuid.Replace("-", "").ToLowerInvariant();
        var formattedUuid = FormatUuid(normalizedUuid);
        var cacheKey = $"mojang:uuid:{formattedUuid.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out MojangProfileDto? cached))
        {
            return cached;
        }

        try
        {
            var url = $"https://sessionserver.mojang.com/session/minecraft/profile/{normalizedUuid}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Player UUID {Uuid} not found in Mojang API", uuid);
                    return null;
                }

                _logger.LogWarning(
                    "Mojang API returned {StatusCode} for UUID lookup: {Uuid}",
                    response.StatusCode,
                    uuid);
                return null;
            }

            var mojangResponse = await response.Content.ReadFromJsonAsync<MojangProfileResponse>(
                JsonOptions,
                cancellationToken);

            if (mojangResponse == null || string.IsNullOrEmpty(mojangResponse.Id))
            {
                return null;
            }

            var profile = new MojangProfileDto(
                formattedUuid,
                mojangResponse.Name,
                string.Format(AvatarUrlTemplate, formattedUuid));

            _cache.Set(cacheKey, profile, CacheDuration);
            _cache.Set($"mojang:username:{mojangResponse.Name.ToLowerInvariant()}", profile, CacheDuration);

            return profile;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to lookup UUID {Uuid} from Mojang API", uuid);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Mojang API response for UUID {Uuid}", uuid);
            return null;
        }
    }

    private static string FormatUuid(string rawUuid)
    {
        var clean = rawUuid.Replace("-", "").ToLowerInvariant();
        if (clean.Length != 32)
        {
            return rawUuid;
        }

        return $"{clean[..8]}-{clean[8..12]}-{clean[12..16]}-{clean[16..20]}-{clean[20..]}";
    }

    private sealed record MojangUsernameResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);

    private sealed record MojangProfileResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}
