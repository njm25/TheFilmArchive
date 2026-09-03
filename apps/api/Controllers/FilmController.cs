using Api.Requests;
using Api.Responses;
using Api.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Clients;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMDbLib.Objects.Movies;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TmdbService _tmdb;
    private readonly FilmSyncService _filmSync;
    private readonly ArchiveOrgService _archiveOrg;

    public FilmController(
        AppDbContext db,
        TmdbService tmdb,
        FilmSyncService filmSync,
        ArchiveOrgService archiveOrg
    )
    {
        _db = db;
        _tmdb = tmdb;
        _filmSync = filmSync;
        _archiveOrg = archiveOrg;
    }

    // GET: api/films
    [HttpGet]
    public async Task<IActionResult> GetFilms([FromQuery] GetFilmsReq req)
    {
        IQueryable<Film> query = _db.Films.AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(req.SearchText))
        {
            query = query.Where(f => f.Title.Contains(req.SearchText));
        }

        if (req.GenreIds != null && req.GenreIds.Count > 0)
        {
            query = query.Where(f => f.Genres.Any(fg => req.GenreIds.Contains(fg.GenreId)));
        }

        if (req.MinRating.HasValue)
        {
            query = query.Where(f => f.VoteAverage >= req.MinRating.Value);
        }

        if (req.MaxRating.HasValue)
        {
            query = query.Where(f => f.VoteAverage <= req.MaxRating.Value);
        }

        if (req.MinYear.HasValue)
        {
            query = query.Where(f => f.ReleaseYear >= req.MinYear.Value);
        }

        if (req.MaxYear.HasValue)
        {
            query = query.Where(f => f.ReleaseYear <= req.MaxYear.Value);
        }

        if (req.MinRuntime.HasValue)
        {
            query = query.Where(f => f.Runtime >= req.MinRuntime.Value);
        }

        if (req.MaxRuntime.HasValue)
        {
            query = query.Where(f => f.Runtime <= req.MaxRuntime.Value);
        }

        if (req.Languages != null && req.Languages.Count > 0)
        {
            query = query.Where(f => f.OriginalLanguage != null && req.Languages.Contains(f.OriginalLanguage));
        }

        // Ordering
        query = (req.OrderBy, req.OrderingType) switch
        {

            (OrderFilmByEnum.YearReleased, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.ReleaseYear).ThenBy(f => f.Id),

            (OrderFilmByEnum.YearReleased, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.ReleaseYear).ThenBy(f => f.Id),

            (OrderFilmByEnum.Title, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.Title).ThenBy(f => f.Id),

            (OrderFilmByEnum.Title, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.Title).ThenBy(f => f.Id),

            (OrderFilmByEnum.Rating, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.VoteAverage).ThenBy(f => f.Id),

            (OrderFilmByEnum.Rating, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.VoteAverage).ThenBy(f => f.Id),

            (OrderFilmByEnum.CreatedAt, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.CreatedAt).ThenBy(f => f.Id),

            (OrderFilmByEnum.CreatedAt, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.CreatedAt).ThenBy(f => f.Id),

            _ => query.OrderByDescending(f => f.VoteAverage).ThenBy(f => f.Id)
        };

        int totalCount = await query.CountAsync();

        // Paging
        int skip = (req.PageNumber - 1) * req.PageSize;

        List<GetFilmResItem> films = await query
            .Skip(skip)
            .Take(req.PageSize)
            .Select(f => new GetFilmResItem
            {
                FilmId = f.Id,
                Title = f.Title,
                YearReleased = f.ReleaseYear ?? 0,
                Description = f.Description ?? "No description found.",
                PosterPath = f.PosterPath ?? "",
                Genres = f.Genres.Select(fg => fg.Genre.Name).ToList(),
                VoteAverage = f.VoteAverage
            })
            .ToListAsync();

        var res = new GetFilmsRes
        {
            Films = films,
            TotalCount = totalCount
        };

        return Ok(res);
    }

    // GET: api/films/genres
    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        List<GetGenreResItem> genres = await _db.Genres
            .OrderBy(g => g.Name)
            .Select(g => new GetGenreResItem
            {
                GenreId = g.Id,
                Name = g.Name
            })
            .ToListAsync();

        return Ok(new GetGenresRes { Genres = genres });
    }

    // GET: api/films/languages
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages()
    {
        List<string> codes = await _db.Films
            .Where(f => f.OriginalLanguage != null)
            .Select(f => f.OriginalLanguage!)
            .Distinct()
            .ToListAsync();

        List<GetLanguageResItem> languages = codes
            .Select(code => new GetLanguageResItem { Code = code, Name = LanguageDisplayName(code) })
            .OrderBy(l => l.Name)
            .ToList();

        return Ok(new GetLanguagesRes { Languages = languages });
    }

    private static string LanguageDisplayName(string code)
    {
        try
        {
            return System.Globalization.CultureInfo.GetCultureInfo(code).EnglishName;
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            return code;
        }
    }

    // GET: api/films/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFilm(int id)
    {
        Film? film = await _db.Films
            .Include(f => f.Sources)
            .Include(f => f.Genres).ThenInclude(fg => fg.Genre)
            .Include(f => f.Credits).ThenInclude(fc => fc.Person)
            .Include(f => f.Collection)
            .Where(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (film == null)
            return NotFound();

        GetFilmRes res = new GetFilmRes()
        {
            Title = film.Title,
            Description = film.Description ?? "No description found.",
            Tagline = film.Tagline ?? "No tagline found.",
            YearReleased = film.ReleaseYear ?? 0,
            PosterPath = film.PosterPath,
            Sources = film.Sources
                    .Select(o => new GetFilmResSource
                    {
                        SourceId = o.Id,
                        Type = o.Type,
                        QualityHeight = o.QualityHeight
                    })
                    .ToList(),
            PrimarySourceTypeId = film
                    .Sources
                    .Where(o => o.IsPrimary)
                    .Select(o => o.Id)
                    .ToList()
                    .FirstOrDefault(-1),
            BackdropPath = film.BackdropPath,
            Runtime = film.Runtime ?? 0,
            ImdbId = film.ImdbId,
            Homepage = film.Homepage,
            Status = film.Status,
            VoteAverage = film.VoteAverage,
            VoteCount = film.VoteCount,
            CollectionName = film.Collection?.Name,
            Genres = film.Genres.Select(fg => fg.Genre.Name).ToList(),
            Directors = film.Credits
                .Where(c => c.CreditType == CreditTypeEnum.Crew && c.Job == "Director")
                .Select(c => c.Person.Name)
                .ToList(),
            Cast = film.Credits
                .Where(c => c.CreditType == CreditTypeEnum.Cast)
                .OrderBy(c => c.CreditOrder ?? int.MaxValue)
                .Take(10)
                .Select(c => new GetFilmResCastMember
                {
                    Name = c.Person.Name,
                    Character = c.Character,
                    ProfilePath = c.Person.ProfilePath
                })
                .ToList()
        };

        return Ok(res);
    }

    // GET: api/films/sources/id
    [HttpGet("sources/{sourceId}")]
    public async Task<IActionResult> GetFilmSource(int sourceId)
    {
        FilmSource? source = await _db.FilmSources
            .AsNoTracking()
            .Where(o => o.Id == sourceId)
            .FirstOrDefaultAsync();

        if (source == null)
            return NotFound();

        if (source.Type != SourceTypeEnum.ArchiveOrg)
        {
            return Ok(new GetFilmSourceRes
            {
                Type = source.Type,
                Url = source.SourceUrl,
                IsDirectVideo = true
            });
        }

        string? identifier = ArchiveOrgService.ExtractIdentifier(source.SourceUrl);
        List<string> playableUrls = identifier != null
            ? await _archiveOrg.GetPlayableFileUrlsAsync(identifier)
            : new List<string>();

        if (playableUrls.Count > 0)
        {
            return Ok(new GetFilmSourceRes
            {
                Type = source.Type,
                Url = playableUrls[0],
                IsDirectVideo = true,
                FallbackUrls = playableUrls.Skip(1).ToList()
            });
        }

        // Fall back to archive.org's own embeddable player when no
        // browser-playable file could be resolved for this item.
        return Ok(new GetFilmSourceRes
        {
            Type = source.Type,
            Url = identifier != null ? $"https://archive.org/embed/{identifier}" : source.SourceUrl,
            IsDirectVideo = false
        });
    }

    // GET: api/films/sources/id/captions
    [HttpGet("sources/{sourceId}/captions")]
    public async Task<IActionResult> GetCaptions(int sourceId)
    {
        string? identifier = await ResolveArchiveOrgIdentifierAsync(sourceId);

        if (identifier == null)
            return Ok(new GetCaptionsRes { Captions = new List<GetCaptionResItem>() });

        List<string> fileNames = await _archiveOrg.GetCaptionFileNamesAsync(identifier);

        // Path only, no scheme or host: behind the TLS-terminating proxy Kestrel
        // sees plain http, so an absolute URL built from Request.Scheme comes back
        // as http:// and the browser blocks it as mixed content on the https site.
        // The client prefixes its own API base URL.
        List<GetCaptionResItem> captions = fileNames.Select(name => new GetCaptionResItem
        {
            Label = ArchiveOrgService.GuessCaptionLabel(name),
            Url = Url.Action(nameof(GetCaptionTrack), "Film", new { sourceId, file = name })!
        }).ToList();

        return Ok(new GetCaptionsRes { Captions = captions });
    }

    // GET: api/films/sources/id/captions/track?file=...
    // Proxies (and, for .srt, converts to WebVTT) a caption file from
    // archive.org - browsers can't load a <track> cross-origin from
    // archive.org's own domain reliably, and can't use .srt at all.
    [HttpGet("sources/{sourceId}/captions/track")]
    public async Task<IActionResult> GetCaptionTrack(int sourceId, [FromQuery] string file)
    {
        string? identifier = await ResolveArchiveOrgIdentifierAsync(sourceId);

        if (identifier == null)
            return NotFound();

        string? vtt = await _archiveOrg.GetCaptionVttAsync(identifier, file);

        if (vtt == null)
            return NotFound();

        return Content(vtt, "text/vtt");
    }

    private async Task<string?> ResolveArchiveOrgIdentifierAsync(int sourceId)
    {
        FilmSource? source = await _db.FilmSources
            .AsNoTracking()
            .Where(o => o.Id == sourceId && o.Type == SourceTypeEnum.ArchiveOrg)
            .FirstOrDefaultAsync();

        return source == null ? null : ArchiveOrgService.ExtractIdentifier(source.SourceUrl);
    }

    // to-do
    private static string BuildPosterUrl(string? posterPath)
    {
        return "";
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPost("addFilm")]
    public async Task<IActionResult> AddFilm([FromBody] AddFilmReq req)
    {
        Movie? movie = await _tmdb.GetMovieByTmdbId(req.TmdbId);

        if (movie == null)
            return NoContent();

        DateTime utc = DateTime.UtcNow;

        Film film = new Film()
        {
            TmdbId = req.TmdbId,
            CreatedAt = utc,
            UpdatedAt = utc,
        };

        await _db.Films.AddAsync(film);

        await _filmSync.ApplyMetadataAsync(film, movie);

        await _db.SaveChangesAsync();

        return Ok(film.Id);
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPost("addSource")]
    public async Task<IActionResult> AddSource([FromBody] AddSourceReq req)
    {
        int? quality = req.QualityHeight;

        if (quality == null && req.SourceType == SourceTypeEnum.ArchiveOrg)
        {
            string? identifier = ArchiveOrgService.ExtractIdentifier(req.SourceUrl);

            if (identifier != null)
            {
                quality = await _archiveOrg.GetBestQualityHeightAsync(identifier);
            }
        }

        FilmSource source = new FilmSource()
        {
            FilmId = req.FilmId,
            SourceUrl = req.SourceUrl,
            Type = req.SourceType,
            QualityHeight = quality,
            CreatedAt = DateTime.UtcNow,
        };

        await _db.FilmSources.AddAsync(source);

        await _db.SaveChangesAsync();

        return Ok(source.Id);
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPost("refreshMetadata/{filmId}")]
    public async Task<IActionResult> RefreshMetadata(string filmId)
    {
        Film? film = await _db.Films.FirstOrDefaultAsync(f => f.Id == int.Parse(filmId));

        if (film == null)
            return NotFound();

        Movie? movie = await _tmdb.GetMovieByTmdbId(film.TmdbId);

        if (movie == null)
            return NotFound();

        await _filmSync.ApplyMetadataAsync(film, movie);

        await _db.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFilm(int id)
    {
        Film? film = await _db.Films.FirstOrDefaultAsync(f => f.Id == id);

        if (film == null)
            return NotFound();

        _db.Films.Remove(film);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpDelete("sources/{sourceId}")]
    public async Task<IActionResult> DeleteSource(int sourceId)
    {
        FilmSource? source = await _db.FilmSources.FirstOrDefaultAsync(s => s.Id == sourceId);

        if (source == null)
            return NotFound();

        _db.FilmSources.Remove(source);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/films/1/logView
    [HttpPost("{id}/logView")]
    public async Task<IActionResult> LogView(int id)
    {
        bool filmExists = await _db.Films.AnyAsync(f => f.Id == id);

        if (!filmExists)
            return NotFound();

        FilmView view = new FilmView
        {
            FilmId = id,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            CreatedAt = DateTime.UtcNow
        };

        _db.FilmViews.Add(view);

        await _db.SaveChangesAsync();

        return Ok();
    }

    // GET: api/films/popular
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular([FromQuery] int take = 12)
    {
        DateTime windowStart = DateTime.UtcNow.AddDays(-30);

        List<int> topFilmIds = await _db.FilmViews
            .Where(v => v.CreatedAt >= windowStart)
            .GroupBy(v => v.FilmId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(take)
            .ToListAsync();

        List<GetFilmResItem> films = await ResolveFilmsInOrderAsync(topFilmIds);

        return Ok(new GetFilmsRes { Films = films, TotalCount = films.Count });
    }

    // GET: api/films/continueWatching
    [Authorize]
    [HttpGet("continueWatching")]
    public async Task<IActionResult> GetContinueWatching([FromQuery] int take = 12)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        List<int> filmIds = await _db.FilmViews
            .Where(v => v.UserId == userId)
            .GroupBy(v => v.FilmId)
            .OrderByDescending(g => g.Max(v => v.CreatedAt))
            .Select(g => g.Key)
            .Take(take)
            .ToListAsync();

        List<GetFilmResItem> films = await ResolveFilmsInOrderAsync(filmIds);

        // The row is ordered by view recency, but how far through each film the
        // user actually is lives in WatchProgress, so it is attached separately.
        Dictionary<int, WatchProgress> progressByFilmId = await _db.WatchProgresses
            .Where(w => w.UserId == userId && filmIds.Contains(w.FilmId))
            .ToDictionaryAsync(w => w.FilmId);

        foreach (GetFilmResItem film in films)
        {
            if (!progressByFilmId.TryGetValue(film.FilmId, out WatchProgress? progress))
                continue;

            if (progress.DurationSeconds <= 0)
                continue;

            film.ProgressSeconds = progress.ProgressSeconds;
            film.DurationSeconds = progress.DurationSeconds;
        }

        return Ok(new GetFilmsRes { Films = films, TotalCount = films.Count });
    }

    // GET: api/films/suggested
    [Authorize]
    [HttpGet("suggested")]
    public async Task<IActionResult> GetSuggested([FromQuery] int take = 12)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        List<int> viewedFilmIds = await _db.FilmViews
            .Where(v => v.UserId == userId)
            .Select(v => v.FilmId)
            .Distinct()
            .ToListAsync();

        if (viewedFilmIds.Count == 0)
            return await GetPopular(take);

        List<int> watchedGenreIds = await _db.FilmGenres
            .Where(fg => viewedFilmIds.Contains(fg.FilmId))
            .Select(fg => fg.GenreId)
            .Distinct()
            .ToListAsync();

        if (watchedGenreIds.Count == 0)
            return await GetPopular(take);

        DateTime windowStart = DateTime.UtcNow.AddDays(-30);

        Dictionary<int, int> viewCounts = await _db.FilmViews
            .Where(v => v.CreatedAt >= windowStart)
            .GroupBy(v => v.FilmId)
            .Select(g => new { FilmId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FilmId, x => x.Count);

        var candidates = await _db.FilmGenres
            .Where(fg => watchedGenreIds.Contains(fg.GenreId) && !viewedFilmIds.Contains(fg.FilmId))
            .GroupBy(fg => fg.FilmId)
            .Select(g => new { FilmId = g.Key, Overlap = g.Count() })
            .ToListAsync();

        List<int> rankedFilmIds = candidates
            .OrderByDescending(x => x.Overlap)
            .ThenByDescending(x => viewCounts.TryGetValue(x.FilmId, out var count) ? count : 0)
            .Take(take)
            .Select(x => x.FilmId)
            .ToList();

        if (rankedFilmIds.Count == 0)
            return await GetPopular(take);

        List<GetFilmResItem> films = await ResolveFilmsInOrderAsync(rankedFilmIds);

        return Ok(new GetFilmsRes { Films = films, TotalCount = films.Count });
    }

    // Fetches the given films and re-orders the results to match filmIds,
    // since order doesn't reliably survive a SQL join after a Take().
    private async Task<List<GetFilmResItem>> ResolveFilmsInOrderAsync(List<int> filmIds)
    {
        Dictionary<int, Film> filmsById = await _db.Films
            .Where(f => filmIds.Contains(f.Id))
            .Include(f => f.Genres).ThenInclude(fg => fg.Genre)
            .ToDictionaryAsync(f => f.Id);

        return filmIds
            .Where(filmsById.ContainsKey)
            .Select(id => filmsById[id])
            .Select(f => new GetFilmResItem
            {
                FilmId = f.Id,
                Title = f.Title,
                YearReleased = f.ReleaseYear ?? 0,
                Description = f.Description ?? "No description found.",
                PosterPath = f.PosterPath ?? "",
                Genres = f.Genres.Select(fg => fg.Genre.Name).ToList(),
                VoteAverage = f.VoteAverage
            })
            .ToList();
    }

    // POST: api/films/watchProgress
    [Authorize]
    [HttpPost("watchProgress")]
    public async Task<IActionResult> UpsertWatchProgress([FromBody] WatchProgressReq req)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        WatchProgress? progress = await _db.WatchProgresses
            .FirstOrDefaultAsync(w => w.UserId == userId && w.FilmId == req.FilmId);

        if (progress == null)
        {
            progress = new WatchProgress
            {
                UserId = userId,
                FilmId = req.FilmId
            };

            _db.WatchProgresses.Add(progress);
        }

        progress.SourceId = req.SourceId;
        progress.ProgressSeconds = req.ProgressSeconds;
        progress.DurationSeconds = req.DurationSeconds;
        progress.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok();
    }

    // GET: api/films/1/watchProgress
    [Authorize]
    [HttpGet("{filmId}/watchProgress")]
    public async Task<IActionResult> GetWatchProgress(int filmId)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        WatchProgress? progress = await _db.WatchProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.FilmId == filmId);

        return Ok(new GetWatchProgressRes
        {
            ProgressSeconds = progress?.ProgressSeconds ?? 0,
            DurationSeconds = progress?.DurationSeconds ?? 0,
            SourceId = progress?.SourceId
        });
    }

}
