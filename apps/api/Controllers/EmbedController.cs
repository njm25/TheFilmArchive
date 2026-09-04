using Api.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace api.Controllers;

// Backs link unfurling for film pages (Discord, iMessage, Slack, Twitter, ...).
// Social crawlers do not execute JavaScript, so the meta tags the Angular SPA
// sets at runtime are invisible to them.
//
// Amplify Hosting cannot branch a rewrite on user agent - its rule conditions
// only cover country - so /film/<id> is rewritten to this controller for every
// visitor. To keep humans on a working page, the response is the real deployed
// index.html with the Open Graph tags injected into its head: crawlers read the
// tags, browsers boot the app exactly as before. If the shell can't be fetched
// the response degrades to a self-contained card that still carries the tags.
[ApiController]
[Route("embed")]
public partial class EmbedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly SpaShellService _shell;

    private const string BackdropBaseUrl = "https://image.tmdb.org/t/p/w1280";
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w780";
    // TMDB only serves profiles at w45, w185, h632 and original - the poster
    // widths above are not valid sizes for a profile path.
    private const string ProfileBaseUrl = "https://image.tmdb.org/t/p/h632";
    private const string DefaultSiteUrl = "https://thefilmarchive.org";
    private const string ThemeColor = "#c9a84c";
    private const string DirectorJob = "Director";
    private const int MaxDescriptionLength = 300;
    private const int MaxNotableFilms = 4;

    public EmbedController(AppDbContext db, IConfiguration config, SpaShellService shell)
    {
        _db = db;
        _config = config;
        _shell = shell;
    }

    // GET: /embed/film/5
    [HttpGet("film/{id}")]
    public async Task<IActionResult> GetFilmEmbed(int id, CancellationToken ct)
    {
        string siteUrl = SiteUrl();

        Film? film = await _db.Films
            .AsNoTracking()
            .Include(f => f.Genres).ThenInclude(fg => fg.Genre)
            .Include(f => f.Credits).ThenInclude(fc => fc.Person)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (film == null)
        {
            HeadTags missing = new HeadTags(
                "Film not found - The Film Archive",
                [
                    MetaName("theme-color", ThemeColor),
                    MetaProperty("og:site_name", "The Film Archive"),
                    MetaProperty("og:type", "website"),
                    MetaProperty("og:url", siteUrl),
                    MetaProperty("og:title", "Film not found"),
                    MetaProperty("og:description", "This film is not in The Film Archive.")
                ]
            );

            return await RenderAsync(missing, siteUrl, $"{siteUrl}/films", StatusCodes.Status404NotFound, ct);
        }

        string canonical = $"{siteUrl}/film/{film.Id}";

        return await RenderAsync(BuildTags(film, canonical), siteUrl, canonical, StatusCodes.Status200OK, ct);
    }

    // GET: /embed/person/5
    [HttpGet("person/{id}")]
    public async Task<IActionResult> GetPersonEmbed(int id, CancellationToken ct)
    {
        string siteUrl = SiteUrl();

        Person? person = await _db.People
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (person == null)
        {
            HeadTags missing = new HeadTags(
                "Person not found - The Film Archive",
                [
                    MetaName("theme-color", ThemeColor),
                    MetaProperty("og:site_name", "The Film Archive"),
                    MetaProperty("og:type", "website"),
                    MetaProperty("og:url", siteUrl),
                    MetaProperty("og:title", "Person not found"),
                    MetaProperty("og:description", "This person is not in The Film Archive.")
                ]
            );

            return await RenderAsync(missing, siteUrl, $"{siteUrl}/films", StatusCodes.Status404NotFound, ct);
        }

        // Only the credits the person page itself shows - a card promising work
        // the page doesn't list would be worse than a thin one. Mirrors the
        // filter in PersonController.
        List<PersonCredit> credits = await _db.FilmCredits
            .AsNoTracking()
            .Where(c => c.PersonId == id
                && (c.CreditType == CreditTypeEnum.Cast
                    || (c.CreditType == CreditTypeEnum.Crew && c.Job == DirectorJob)))
            .Select(c => new PersonCredit(
                c.CreditType,
                c.FilmId,
                c.Film.Title,
                c.Film.ReleaseYear,
                c.Film.VoteAverage
            ))
            .ToListAsync(ct);

        string canonical = $"{siteUrl}/person/{person.Id}";

        return await RenderAsync(BuildTags(person, credits, canonical), siteUrl, canonical, StatusCodes.Status200OK, ct);
    }

    // Prefers the deployed app shell so browsers get a working page, and falls
    // back to a static card carrying the same tags when it isn't reachable.
    private async Task<IActionResult> RenderAsync(
        HeadTags tags,
        string siteUrl,
        string canonical,
        int statusCode,
        CancellationToken ct
    )
    {
        string? shell = await _shell.GetShellAsync(ct);

        string html = shell != null
            ? InjectIntoShell(shell, tags, siteUrl, canonical)
            : BuildFallbackHtml(tags, canonical);

        // Short enough that a frontend deploy isn't masked by a stale unfurl cache.
        Response.Headers.CacheControl = "public, max-age=300";

        return new ContentResult
        {
            Content = html,
            ContentType = "text/html; charset=utf-8",
            StatusCode = statusCode
        };
    }

    private static string InjectIntoShell(string shell, HeadTags tags, string siteUrl, string canonical)
    {
        // The shell ships one <title>; replacing it keeps crawlers that read the
        // title element rather than og:title in agreement with the rest.
        string html = TitleTag().Replace(shell, $"<title>{Encode(tags.Title)}</title>", 1);

        StringBuilder head = new StringBuilder();
        foreach (string tag in tags.Tags)
            head.AppendLine(tag);

        // The shell's relative asset paths only resolve on the site origin, so a
        // browser that reaches this response on the API host is sent back over.
        // Crawlers don't run scripts, and on the site origin this is a no-op.
        head.AppendLine(
            "<script>if(location.origin!==" + JsonSerializer.Serialize(siteUrl) +
            ")location.replace(" + JsonSerializer.Serialize(canonical) + ");</script>"
        );

        int close = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);

        return html[..close] + head.ToString() + html[close..];
    }

    private static string BuildFallbackHtml(HeadTags tags, string canonical)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine($"<title>{Encode(tags.Title)}</title>");

        foreach (string tag in tags.Tags)
            html.AppendLine(tag);

        html.AppendLine(Styles());
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<main>");
        html.AppendLine($"<h1>{Encode(tags.Heading)}</h1>");
        html.AppendLine($"<p>{Encode(tags.Description)}</p>");
        html.AppendLine($"<p><a href=\"{Encode(canonical)}\">Open The Film Archive</a></p>");
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static HeadTags BuildTags(Film film, string canonical)
    {
        string heading = film.ReleaseYear.HasValue
            ? $"{film.Title} ({film.ReleaseYear})"
            : film.Title;

        // Backdrops are 16:9 and fill a wide card nicely; a poster in that slot
        // gets letterboxed, so fall back to the small card when that is all we have.
        bool hasBackdrop = !string.IsNullOrWhiteSpace(film.BackdropPath);

        string? image = hasBackdrop
            ? BackdropBaseUrl + film.BackdropPath
            : !string.IsNullOrWhiteSpace(film.PosterPath)
                ? PosterBaseUrl + film.PosterPath
                : null;

        string description = BuildDescription(film);

        List<string> tags =
        [
            $"<link rel=\"canonical\" href=\"{Encode(canonical)}\">",
            MetaName("description", description),
            MetaName("theme-color", ThemeColor),
            MetaProperty("og:site_name", "The Film Archive"),
            MetaProperty("og:type", "video.movie"),
            MetaProperty("og:url", canonical),
            MetaProperty("og:title", heading),
            MetaProperty("og:description", description)
        ];

        if (image != null)
        {
            tags.Add(MetaProperty("og:image", image));
            tags.Add(MetaProperty("og:image:secure_url", image));
            tags.Add(MetaProperty("og:image:type", "image/jpeg"));
            tags.Add(MetaProperty("og:image:width", (hasBackdrop ? 1280 : 780).ToString(CultureInfo.InvariantCulture)));
            tags.Add(MetaProperty("og:image:height", (hasBackdrop ? 720 : 1170).ToString(CultureInfo.InvariantCulture)));
            tags.Add(MetaProperty("og:image:alt", $"{film.Title} artwork"));
        }

        if (film.ReleaseYear.HasValue)
            tags.Add(MetaProperty("video:release_date", $"{film.ReleaseYear}-01-01"));

        if (film.Runtime.HasValue && film.Runtime > 0)
            tags.Add(MetaProperty("video:duration", (film.Runtime.Value * 60).ToString(CultureInfo.InvariantCulture)));

        foreach (string director in Directors(film))
            tags.Add(MetaProperty("video:director", director));

        foreach (string genre in film.Genres.Select(fg => fg.Genre.Name))
            tags.Add(MetaProperty("video:tag", genre));

        tags.Add(MetaName("twitter:card", hasBackdrop ? "summary_large_image" : "summary"));
        tags.Add(MetaName("twitter:title", heading));
        tags.Add(MetaName("twitter:description", description));

        if (image != null)
            tags.Add(MetaName("twitter:image", image));

        return new HeadTags($"{heading} - The Film Archive", tags, heading, description);
    }

    // Crawlers render this as one block of text under the title, so the first
    // line carries the facts and the second the synopsis.
    private static string BuildDescription(Film film)
    {
        // No release year here - the heading above already carries it as
        // "Title (1942)", and repeating it opens the fact line with a number
        // the reader just read.
        List<string> facts = new List<string>();

        // Leads the line: of everything here it's the one fact that says most
        // about the film, and it reads as a byline ahead of the numbers.
        List<string> directors = Directors(film).Take(2).ToList();
        if (directors.Count > 0)
            facts.Add($"Dir. {string.Join(", ", directors)}");

        string runtime = FormatRuntime(film.Runtime ?? 0);
        if (runtime.Length > 0)
            facts.Add(runtime);

        List<string> genres = film.Genres
            .Select(fg => fg.Genre.Name)
            .Take(3)
            .ToList();

        if (genres.Count > 0)
            facts.Add(string.Join(", ", genres));

        if (film.VoteAverage.HasValue && film.VoteAverage > 0)
            facts.Add($"★ {film.VoteAverage.Value.ToString("0.0", CultureInfo.InvariantCulture)}");

        // The synopsis tells a reader deciding whether to watch more than a
        // tagline does, so it leads; the tagline is the fallback.
        string synopsis = !string.IsNullOrWhiteSpace(film.Description)
            ? film.Description!
            : film.Tagline ?? string.Empty;

        synopsis = Truncate(synopsis.Trim(), MaxDescriptionLength);

        string factLine = string.Join(" · ", facts);

        if (factLine.Length > 0 && synopsis.Length > 0)
            return $"{factLine}\n\n{synopsis}";

        return factLine.Length > 0 ? factLine : synopsis;
    }

    private static HeadTags BuildTags(Person person, List<PersonCredit> credits, string canonical)
    {
        string heading = person.Name;
        string description = BuildDescription(person, credits);

        string? image = !string.IsNullOrWhiteSpace(person.ProfilePath)
            ? ProfileBaseUrl + person.ProfilePath
            : null;

        List<string> tags =
        [
            $"<link rel=\"canonical\" href=\"{Encode(canonical)}\">",
            MetaName("description", description),
            MetaName("theme-color", ThemeColor),
            MetaProperty("og:site_name", "The Film Archive"),
            MetaProperty("og:type", "profile"),
            MetaProperty("og:url", canonical),
            MetaProperty("og:title", heading),
            MetaProperty("og:description", description)
        ];

        if (image != null)
        {
            tags.Add(MetaProperty("og:image", image));
            tags.Add(MetaProperty("og:image:secure_url", image));
            tags.Add(MetaProperty("og:image:type", "image/jpeg"));
            // No width/height: TMDB's h632 profiles are a fixed height with a
            // width that varies per photo, so any pair here would be a guess.
            tags.Add(MetaProperty("og:image:alt", $"{person.Name} portrait"));
        }

        // A portrait letterboxes badly in a wide card, so people always get the
        // small card - the same call the film side makes for a poster-only film.
        tags.Add(MetaName("twitter:card", "summary"));
        tags.Add(MetaName("twitter:title", heading));
        tags.Add(MetaName("twitter:description", description));

        if (image != null)
            tags.Add(MetaName("twitter:image", image));

        return new HeadTags($"{heading} - The Film Archive", tags, heading, description);
    }

    // Same two-part shape as a film's: the facts a card can show at a glance,
    // then the titles that say who this person actually is.
    private static string BuildDescription(Person person, List<PersonCredit> credits)
    {
        // Someone who directed and starred in a film holds two credits on it,
        // and the count is of films either way.
        int filmCount = credits.Select(c => c.FilmId).Distinct().Count();

        if (filmCount == 0)
            return $"{person.Name} has no films in The Film Archive yet.";

        List<string> facts = new List<string>();

        if (credits.Any(c => c.CreditType == CreditTypeEnum.Crew))
            facts.Add("Director");

        if (credits.Any(c => c.CreditType == CreditTypeEnum.Cast))
            facts.Add("Actor");

        facts.Add(filmCount == 1 ? "1 film" : $"{filmCount} films");

        // Best-rated first: with only a handful of titles to spend, the ones a
        // reader is likeliest to recognise earn the space.
        List<string> notable = credits
            .GroupBy(c => c.FilmId)
            .Select(g => g.First())
            .OrderByDescending(c => c.VoteAverage ?? 0)
            .ThenByDescending(c => c.ReleaseYear ?? 0)
            .Take(MaxNotableFilms)
            .Select(c => c.ReleaseYear.HasValue ? $"{c.Title} ({c.ReleaseYear})" : c.Title)
            .ToList();

        string factLine = string.Join(" · ", facts);
        string titleLine = Truncate(string.Join(", ", notable), MaxDescriptionLength);

        return $"{factLine}\n\n{titleLine}";
    }

    private static List<string> Directors(Film film) => film.Credits
        .Where(c => c.CreditType == CreditTypeEnum.Crew && c.Job == DirectorJob)
        .Select(c => c.Person.Name)
        .ToList();

    private static string FormatRuntime(int minutes)
    {
        if (minutes <= 0)
            return string.Empty;

        int hours = minutes / 60;
        int remainder = minutes % 60;

        if (hours == 0)
            return $"{remainder}m";

        return remainder == 0 ? $"{hours}h" : $"{hours}h {remainder}m";
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        int cut = value.LastIndexOf(' ', maxLength - 1);
        if (cut < maxLength / 2)
            cut = maxLength - 1;

        return value[..cut].TrimEnd(',', '.', ';', ':', ' ') + "…";
    }

    private string SiteUrl() =>
        (_config["Site:BaseUrl"] ?? DefaultSiteUrl).TrimEnd('/');

    // Newlines are legal inside an attribute value but survive proxies and
    // minifiers more reliably as a character reference.
    private static string Encode(string value) =>
        WebUtility.HtmlEncode(value).Replace("\n", "&#10;");

    private static string MetaProperty(string property, string content) =>
        $"<meta property=\"{property}\" content=\"{Encode(content)}\">";

    private static string MetaName(string name, string content) =>
        $"<meta name=\"{name}\" content=\"{Encode(content)}\">";

    private static string Styles() =>
        "<style>body{margin:0;background:#0f0f10;color:#e8e6e1;" +
        "font:16px/1.6 system-ui,-apple-system,Segoe UI,sans-serif}" +
        "main{max-width:42rem;margin:0 auto;padding:3rem 1.5rem}" +
        "h1{font-size:1.6rem;margin:0 0 .75rem}" +
        "p{margin:0 0 1rem;white-space:pre-line}" +
        "a{color:#c9a84c}</style>";

    [GeneratedRegex(@"<title>.*?</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleTag();

    // Flattened at the query so a person's card can be built without pulling
    // whole Film rows back for credits that only contribute a title.
    private sealed record PersonCredit(
        CreditTypeEnum CreditType,
        int FilmId,
        string Title,
        int? ReleaseYear,
        double? VoteAverage
    );

    private sealed record HeadTags(
        string Title,
        List<string> Tags,
        string Heading = "",
        string Description = ""
    );
}
