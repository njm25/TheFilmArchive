using Api.Requests;
using Api.Responses;
using Domain.Entities;
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

    public FilmController(
        AppDbContext db,
        TmdbService tmdb
    )
    {
        _db = db;
        _tmdb = tmdb;

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
                PosterUrl = f.PosterPath ?? ""
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
            PosterUrl = BuildPosterUrl(film.PosterPath) ?? "No path found.",
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
                    .FirstOrDefault(-1)

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
            Title = movie.Title ?? "No title found",
            Tagline = movie.Tagline,
            Description = movie.Overview,
            PosterPath = movie.PosterPath,
            ReleaseYear = movie.ReleaseDate?.Year,
            CreatedAt = utc,
            UpdatedAt = utc
        };

        await _db.Films.AddAsync(film);

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

}
