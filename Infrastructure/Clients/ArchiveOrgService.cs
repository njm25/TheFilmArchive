using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Clients;

/// <summary>
/// Looks up the best known video quality for an archive.org item via its
/// public metadata API (https://archive.org/metadata/{identifier}), which
/// lists every file the item hosts along with real width/height for video
/// files. Used to auto-detect source quality instead of guessing from the
/// URL/title.
/// </summary>
public class ArchiveOrgService
{
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
        try
        {
            using HttpResponseMessage response = await _client.GetAsync($"https://archive.org/metadata/{identifier}");

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("files", out JsonElement files) || files.ValueKind != JsonValueKind.Array)
                return null;

            int? best = null;

            foreach (JsonElement file in files.EnumerateArray())
            {
                if (!file.TryGetProperty("height", out JsonElement heightEl))
                    continue;

                int? height = heightEl.ValueKind switch
                {
                    JsonValueKind.String => int.TryParse(heightEl.GetString(), out int parsed) ? parsed : (int?)null,
                    JsonValueKind.Number => heightEl.GetInt32(),
                    _ => null
                };

                if (height.HasValue && (best == null || height > best))
                    best = height;
            }

            return best;
        }
        catch
        {
            return null;
        }
    }
}
