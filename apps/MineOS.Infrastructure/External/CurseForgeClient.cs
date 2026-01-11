using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MineOS.Application.Options;

namespace MineOS.Infrastructure.External;

public sealed class CurseForgeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly CurseForgeOptions _options;
    private readonly ILogger<CurseForgeClient> _logger;

    public CurseForgeClient(
        HttpClient httpClient,
        IOptions<CurseForgeOptions> options,
        ILogger<CurseForgeClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<CurseForgeApiResponse<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-api-key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("CurseForge API error ({Status}): {Body}", response.StatusCode, payload);
            throw new HttpRequestException($"CurseForge API error ({(int)response.StatusCode}): {payload}");
        }

        var parsed = JsonSerializer.Deserialize<CurseForgeApiResponse<T>>(payload, JsonOptions);
        if (parsed == null)
        {
            throw new InvalidOperationException("CurseForge API response was empty");
        }

        return parsed;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("CurseForge API key is not configured");
        }
    }
}

public sealed class CurseForgeApiResponse<T>
{
    public T Data { get; set; } = default!;
    public CurseForgePagination? Pagination { get; set; }
}

public sealed class CurseForgePagination
{
    public int Index { get; set; }
    public int PageSize { get; set; }
    public int ResultCount { get; set; }
    public int TotalCount { get; set; }
}
