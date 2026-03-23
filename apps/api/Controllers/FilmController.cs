using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmController : ControllerBase
{
    private readonly AppDbContext _db;

    public FilmController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/films
    [HttpGet]
    public async Task<IActionResult> GetFilms()
    {
        var films = await _db.Films
            .Select(f => new
            {
                f.Id,
                f.Title,
                f.ReleaseYear,
                f.PosterPath
            })
            .ToListAsync();

        return Ok(films);
    }

    // GET: api/films/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFilm(int id)
    {
        var film = await _db.Films
            .Include(f => f.Sources)
            .Where(f => f.Id == id)
            .Select(f => new
            {
                f.Id,
                f.Title,
                f.ReleaseYear,
                f.Description,
                f.PosterPath,
                Sources = f.Sources.Select(s => new
                {
                    s.Id,
                    s.Type,
                    s.SourceUrl,
                    s.IsPrimary,
                    s.IsActive
                })
            })
            .FirstOrDefaultAsync();

        if (film == null)
            return NotFound();

        return Ok(film);
    }
}
