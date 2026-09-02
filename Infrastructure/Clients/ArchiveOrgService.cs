using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Clients;

/// <summary>
/// Looks up file info for an archive.org item via its public metadata API
/// (https://archive.org/metadata/{identifier}), which lists every file the
/// item hosts along with real width/height for video files. Used both to
/// auto-detect source quality and to resolve a direct, browser-playable
/// video file URL instead of relying on archive.org's own iframe embed
/// (which exposes no playback events to the page embedding it).
/// </summary>
public class ArchiveOrgService
{
    private static readonly string[] PlayableExtensions = [".mp4", ".webm", ".ogv"];

    private readonly HttpClient _client;

    public ArchiveOrgService(HttpClient client)
    {
        _client = client;
    }

    public static string? ExtractIdentifier(string url)
    {
        Match match = Regex.Match(url, @"archive\.org/(?:details|embed)/([^/?#]+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    public async Task<int?> GetBestQualityHeightAsync(string identifier)
    {
        List<JsonElement>? files = await GetFilesAsync(identifier);

        if (files == null)
            return null;

        int? best = null;

        foreach (JsonElement file in files)
        {
            int? height = ReadHeight(file);

            if (height.HasValue && (best == null || height > best))
                best = height;
        }

        return best;
    }

    /// Picks the highest-quality browser-playable (mp4/webm/ogv) file for
    /// the item and returns its direct download URL, or null if the item
    /// has no such derivative (e.g. still processing, audio-only, or an
    /// unusually old/rare format).
    public async Task<string?> GetBestPlayableFileUrlAsync(string identifier)
    {
        List<JsonElement>? files = await GetFilesAsync(identifier);

        if (files == null)
            return null;

        string? bestName = null;
        int bestHeight = -1;

        foreach (JsonElement file in files)
        {
            if (!file.TryGetProperty("name", out JsonElement nameEl) || nameEl.ValueKind != JsonValueKind.String)
                continue;

            string? name = nameEl.GetString();

            if (name == null || !IsPlayableVideoFile(name))
                continue;

            int height = ReadHeight(file) ?? 0;

            if (height > bestHeight)
            {
                bestHeight = height;
                bestName = name;
            }
        }

        return bestName == null
            ? null
            : $"https://archive.org/download/{identifier}/{Uri.EscapeDataString(bestName)}";
    }

    private static bool IsPlayableVideoFile(string name) =>
        PlayableExtensions.Any(ext => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

    private static int? ReadHeight(JsonElement file)
    {
        if (!file.TryGetProperty("height", out JsonElement heightEl))
            return null;

        return heightEl.ValueKind switch
        {
            JsonValueKind.String => int.TryParse(heightEl.GetString(), out int parsed) ? parsed : (int?)null,
            JsonValueKind.Number => heightEl.GetInt32(),
            _ => null
        };
    }

    private async Task<List<JsonElement>?> GetFilesAsync(string identifier)
    {
        try
        {
            using HttpResponseMessage response = await _client.GetAsync($"https://archive.org/metadata/{identifier}");

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("files", out JsonElement files) || files.ValueKind != JsonValueKind.Array)
                return null;

            // Clone each element so it stays valid after `doc` is disposed.
            return files.EnumerateArray().Select(f => f.Clone()).ToList();
        }
        catch
        {
            return null;
        }
    }
}
