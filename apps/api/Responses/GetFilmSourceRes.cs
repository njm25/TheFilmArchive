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
}
