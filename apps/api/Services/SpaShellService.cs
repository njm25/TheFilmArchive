using Microsoft.Extensions.Caching.Memory;

namespace Api.Services;

// Fetches the deployed Angular index.html so the API can hand back the real app
// shell with Open Graph tags injected into its head. The shell is cached because
// it only changes on a frontend deploy, and every film page load goes through it.
public class SpaShellService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SpaShellService> _logger;
    private readonly string _shellUrl;

    private const string CacheKey = "spa-shell";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public SpaShellService(
        HttpClient http,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<SpaShellService> logger
    )
    {
        _http = http;
        _cache = cache;
        _logger = logger;

        string baseUrl = (config["Site:BaseUrl"] ?? "https://thefilmarchive.org").TrimEnd('/');
        _shellUrl = config["Site:ShellUrl"] ?? $"{baseUrl}/index.html";

        _http.Timeout = TimeSpan.FromSeconds(5);
    }

    // Returns null when the shell can't be fetched or doesn't look like HTML, so
    // callers can fall back to a self-contained page rather than serving garbage.
    public async Task<string?> GetShellAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out string? cached))
            return cached;

        try
        {
            string html = await _http.GetStringAsync(_shellUrl, ct);

            if (!html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("SPA shell at {ShellUrl} has no </head>; ignoring it.", _shellUrl);
                return null;
            }

            _cache.Set(CacheKey, html, CacheTtl);
            return html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch the SPA shell from {ShellUrl}.", _shellUrl);
            return null;
        }
    }
}
