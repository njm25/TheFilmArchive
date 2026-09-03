using Domain.Enums;

namespace Api.Responses;

public class GetFilmSourceRes
{
    public SourceTypeEnum Type { get; set; }

    public required string Url { get; set; }

    /// True when Url is a direct, browser-playable video file (works in a
    /// plain &lt;video&gt; element). False only for the rare archive.org item
    /// with no resolvable playable derivative, where Url is instead
    /// archive.org's own embeddable player page.
    public bool IsDirectVideo { get; set; }

    /// Other playable derivatives of the same item, in priority order, for
    /// the player to fall back through if Url turns out not to actually
    /// decode in the browser (empty for S3 sources, which have only one file).
    public List<string> FallbackUrls { get; set; } = new();
}
