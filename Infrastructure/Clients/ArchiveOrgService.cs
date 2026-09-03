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

    // archive.org labels subtitle/caption derivatives with one of these two
    // formats in its metadata - "Web Video Text Tracks" for .vtt (playable
    // as-is by a <track> element) and "SubRip" for .srt (needs converting,
    // since browsers don't support .srt as a <track> source).
    private static readonly string[] CaptionFormats = ["Web Video Text Tracks", "SubRip"];

    private static readonly Dictionary<string, string> LanguageNamesByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "English",
        ["es"] = "Spanish",
        ["fr"] = "French",
        ["de"] = "German",
        ["it"] = "Italian",
        ["pt"] = "Portuguese",
        ["ru"] = "Russian",
        ["ja"] = "Japanese",
        ["zh"] = "Chinese",
        ["nl"] = "Dutch",
        ["sv"] = "Swedish",
        ["pl"] = "Polish",
        ["ar"] = "Arabic"
    };

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

    /// Lists every browser-playable (mp4/webm/ogv) derivative for the item as
    /// direct download URLs, ordered highest-quality first. The caller should
    /// treat this as an ordered list of candidates, not just a single pick -
    /// the highest-resolution derivative occasionally turns out to have no
    /// decodable video track in a given browser (seen on some very large,
    /// oddly-encoded archive.org uploads) despite otherwise looking fine, so
    /// the player falls back through the rest of the list when that happens.
    public async Task<List<string>> GetPlayableFileUrlsAsync(string identifier)
    {
        List<JsonElement>? files = await GetFilesAsync(identifier);

        if (files == null)
            return new List<string>();

        List<(string Name, int Height)> candidates = new();

        foreach (JsonElement file in files)
        {
            if (!file.TryGetProperty("name", out JsonElement nameEl) || nameEl.ValueKind != JsonValueKind.String)
                continue;

            string? name = nameEl.GetString();

            if (name == null || !IsPlayableVideoFile(name))
                continue;

            candidates.Add((name, ReadHeight(file) ?? 0));
        }

        return candidates
            .OrderByDescending(c => c.Height)
            .Select(c => $"https://archive.org/download/{identifier}/{Uri.EscapeDataString(c.Name)}")
            .ToList();
    }

    /// Lists the item's subtitle/caption files (as archive.org file names,
    /// not full URLs) by their declared metadata format, in whatever order
    /// archive.org returns them.
    public async Task<List<string>> GetCaptionFileNamesAsync(string identifier)
    {
        List<JsonElement>? files = await GetFilesAsync(identifier);

        if (files == null)
            return new List<string>();

        List<string> names = new();

        foreach (JsonElement file in files)
        {
            if (!file.TryGetProperty("name", out JsonElement nameEl) || nameEl.ValueKind != JsonValueKind.String)
                continue;

            if (!file.TryGetProperty("format", out JsonElement formatEl) || formatEl.ValueKind != JsonValueKind.String)
                continue;

            string? name = nameEl.GetString();
            string? format = formatEl.GetString();

            if (name != null && format != null && CaptionFormats.Contains(format, StringComparer.OrdinalIgnoreCase))
                names.Add(name);
        }

        return names;
    }

    /// Fetches a caption file's raw content and returns it as WebVTT text,
    /// converting from SubRip (.srt) if that's the source format - browsers
    /// only accept WebVTT for a <track> element's src.
    public async Task<string?> GetCaptionVttAsync(string identifier, string fileName)
    {
        try
        {
            string url = $"https://archive.org/download/{identifier}/{Uri.EscapeDataString(fileName)}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string content = await response.Content.ReadAsStringAsync();

            return fileName.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase)
                ? content
                : ConvertSrtToVtt(content);
        }
        catch
        {
            return null;
        }
    }

    /// Guesses a human-readable label for a caption file from a language
    /// code embedded in its name (e.g. "movie.en.srt") - falls back to a
    /// generic label when none is recognized.
    public static string GuessCaptionLabel(string fileName)
    {
        Match match = Regex.Match(fileName, @"[._-](en|es|fr|de|it|pt|ru|ja|zh|nl|sv|pl|ar)(?=[._-]|\.[A-Za-z0-9]+$)");

        return match.Success && LanguageNamesByCode.TryGetValue(match.Groups[1].Value, out string? language)
            ? language
            : "Subtitles";
    }

    private static string ConvertSrtToVtt(string srt)
    {
        string body = srt.Replace("\r\n", "\n").TrimStart('﻿');

        // WebVTT uses a period for the milliseconds separator; SubRip uses a comma.
        body = Regex.Replace(body, @"(\d{2}:\d{2}:\d{2}),(\d{3})", "$1.$2");

        return "WEBVTT\n\n" + body;
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
