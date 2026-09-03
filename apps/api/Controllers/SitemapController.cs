using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml;

namespace api.Controllers;

// The catalog changes without a frontend deploy, so the sitemap is generated
// from the database on request rather than baked in at build time. Amplify
// rewrites https://thefilmarchive.org/sitemap.xml onto this endpoint - without
// that rule the request falls through to the SPA and returns index.html with a
// 200, which is what crawlers were getting before.
[ApiController]
public class SitemapController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    private const string DefaultSiteUrl = "https://thefilmarchive.org";
    private const string Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public SitemapController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // GET: /sitemap.xml
    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> GetSitemap(CancellationToken ct)
    {
        string siteUrl = (_config["Site:BaseUrl"] ?? DefaultSiteUrl).TrimEnd('/');

        var films = await _db.Films
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .Select(f => new { f.Id, f.UpdatedAt })
            .ToListAsync(ct);

        // Written to a UTF-8 stream rather than a StringBuilder: a StringBuilder
        // is UTF-16, and XmlWriter would stamp encoding="utf-16" on a document
        // that is then served as UTF-8, which strict parsers reject.
        using MemoryStream buffer = new MemoryStream();

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
            Async = false
        };

        using (XmlWriter writer = XmlWriter.Create(buffer, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", Namespace);

            WriteUrl(writer, $"{siteUrl}/", null, "daily", "1.0");
            WriteUrl(writer, $"{siteUrl}/films", null, "daily", "0.9");
            WriteUrl(writer, $"{siteUrl}/about", null, "monthly", "0.4");

            foreach (var film in films)
                WriteUrl(writer, $"{siteUrl}/film/{film.Id}", film.UpdatedAt, "weekly", "0.8");

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        Response.Headers.CacheControl = "public, max-age=3600";

        return File(buffer.ToArray(), "application/xml; charset=utf-8");
    }

    private static void WriteUrl(
        XmlWriter writer,
        string location,
        DateTime? lastModified,
        string changeFrequency,
        string priority
    )
    {
        writer.WriteStartElement("url");
        writer.WriteElementString("loc", location);

        if (lastModified.HasValue && lastModified.Value != default)
        {
            writer.WriteElementString(
                "lastmod",
                lastModified.Value.ToUniversalTime().ToString("yyyy-MM-dd")
            );
        }

        writer.WriteElementString("changefreq", changeFrequency);
        writer.WriteElementString("priority", priority);
        writer.WriteEndElement();
    }
}
