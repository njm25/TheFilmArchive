using Api.Responses;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

// People come in through the TMDB metadata import as film credits, never on
// their own, so there is no browse-all-people endpoint here - a person is only
// ever reached from a film they are credited on.
[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly AppDbContext _db;

    private const string DirectorJob = "Director";

    public PersonController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Person/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPerson(int id, CancellationToken ct)
    {
        Person? person = await _db.People
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (person == null)
            return NotFound();

        // Every credit that puts this person on a film in a role the site
        // surfaces, in one trip - directing and acting are the only two the
        // import gives us anything to show, so crew credits are narrowed to
        // Director rather than fetched and thrown away.
        var credits = await _db.FilmCredits
            .AsNoTracking()
            .Where(c => c.PersonId == id
                && (c.CreditType == CreditTypeEnum.Cast
                    || (c.CreditType == CreditTypeEnum.Crew && c.Job == DirectorJob)))
            .Select(c => new
            {
                c.CreditType,
                c.Character,
                FilmId = c.Film.Id,
                c.Film.Title,
                c.Film.ReleaseYear,
                c.Film.Description,
                c.Film.PosterPath,
                c.Film.VoteAverage,
                Genres = c.Film.Genres.Select(fg => fg.Genre.Name).ToList()
            })
            .ToListAsync(ct);

        // Grouped by film because TMDB can credit the same person on one film
        // more than once - two "Director" rows, or an actor billed twice for a
        // dual role - and a filmography should list the film once either way.
        // Character is only ever set on cast credits, so the crew list comes
        // out of the same shaping with nothing extra on it.
        List<GetPersonResFilm> Filmography(CreditTypeEnum creditType) => credits
            .Where(c => c.CreditType == creditType)
            .GroupBy(c => c.FilmId)
            .Select(g =>
            {
                var credit = g.First();

                return new GetPersonResFilm
                {
                    FilmId = g.Key,
                    Title = credit.Title,
                    YearReleased = credit.ReleaseYear ?? 0,
                    Description = credit.Description ?? "No description found.",
                    PosterPath = credit.PosterPath ?? "",
                    VoteAverage = credit.VoteAverage,
                    Genres = credit.Genres,
                    Character = JoinCharacters(g.Select(c => c.Character))
                };
            })
            .OrderByDescending(f => f.YearReleased)
            .ThenBy(f => f.Title)
            .ToList();

        GetPersonRes res = new GetPersonRes
        {
            PersonId = person.Id,
            Name = person.Name,
            ProfilePath = person.ProfilePath,
            Directed = Filmography(CreditTypeEnum.Crew),
            Acted = Filmography(CreditTypeEnum.Cast)
        };

        return Ok(res);
    }

    // A dual role gives one cast credit per character; the two read better on
    // the page as a single "Jekyll / Hyde" line than as the film listed twice.
    private static string? JoinCharacters(IEnumerable<string?> characters)
    {
        string joined = string.Join(" / ", characters.Where(c => !string.IsNullOrWhiteSpace(c)));

        return string.IsNullOrEmpty(joined) ? null : joined;
    }
}
