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
using TMDbLib.Objects.Movies;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TmdbService _tmdb;
    private readonly FilmSyncService _filmSync;

    public FilmController(
        AppDbContext db,
        TmdbService tmdb,
        FilmSyncService filmSync
    )
    {
        _db = db;
        _tmdb = tmdb;
        _filmSync = filmSync;

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

        // Ordering
        query = (req.OrderBy, req.OrderingType) switch
        {

            (OrderFilmByEnum.YearReleased, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.ReleaseYear),

            (OrderFilmByEnum.YearReleased, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.ReleaseYear),

            _ => query.OrderBy(f => f.Id)
        };

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
            Films = films
        };

        return Ok(res);
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
            PosterPath = film.PosterPath ?? "No path found.",
            Sources = film.Sources
                    .Select(o => new GetFilmResSource
                    {
                        SourceId = o.Id,
                        Type = o.Type
                    })
                    .ToList(),
            PrimarySourceTypeId = film
                    .Sources
                    .Where(o => o.IsPrimary)
                    .Select(o => o.Id)
                    .ToList()
                    .FirstOrDefault(-1),
            BackdropPath = film.BackdropPath ?? "No path found.",
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

        return Ok(source.SourceUrl);
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
        FilmSource source = new FilmSource()
        {
            FilmId = req.FilmId,
            SourceUrl = req.SourceUrl,
            Type = req.SourceType,
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

}
