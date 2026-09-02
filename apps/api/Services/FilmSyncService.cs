using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TMDbLib.Objects.Movies;

namespace Api.Services;

/// <summary>
/// Maps a TMDbLib Movie graph (base fields + credits/keywords/videos/
/// release_dates/alternative_titles fetched via append_to_response) onto
/// the Film aggregate and its related entities, upserting shared reference
/// entities (Person/Genre/Keyword/ProductionCompany/Collection) by TmdbId.
/// </summary>
public class FilmSyncService
{
    private readonly AppDbContext _db;

    public FilmSyncService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ApplyMetadataAsync(Film film, Movie movie)
    {
        ApplyScalarFields(film, movie);

        film.Collection = await ResolveCollectionAsync(movie);

        if (film.Id != 0)
        {
            await ClearExistingChildRowsAsync(film.Id);
        }

        await SyncGenresAsync(film, movie);
        await SyncKeywordsAsync(film, movie);
        await SyncProductionCompaniesAsync(film, movie);
        await SyncCreditsAsync(film, movie);
        SyncAlternativeTitles(film, movie);
        SyncVideos(film, movie);
        SyncReleaseDates(film, movie);

        film.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyScalarFields(Film film, Movie movie)
    {
        film.Title = movie.Title ?? film.Title;
        film.OriginalTitle = movie.OriginalTitle;
        film.OriginalLanguage = movie.OriginalLanguage;
        film.Tagline = movie.Tagline;
        film.Description = movie.Overview;
        film.Homepage = movie.Homepage;
        film.ImdbId = movie.ImdbId;
        film.Status = MapStatus(movie.Status);
        film.Adult = movie.Adult;
        film.Budget = movie.Budget;
        film.Revenue = movie.Revenue;
        film.Popularity = movie.Popularity;
        film.VoteAverage = movie.VoteAverage;
        film.VoteCount = movie.VoteCount;
        film.PosterPath = movie.PosterPath;
        film.BackdropPath = movie.BackdropPath;
        film.ReleaseYear = movie.ReleaseDate?.Year;
        film.Runtime = movie.Runtime;
    }

    private async Task ClearExistingChildRowsAsync(int filmId)
    {
        _db.FilmCredits.RemoveRange(await _db.FilmCredits.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmGenres.RemoveRange(await _db.FilmGenres.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmKeywords.RemoveRange(await _db.FilmKeywords.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmProductionCompanies.RemoveRange(await _db.FilmProductionCompanies.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmAlternativeTitles.RemoveRange(await _db.FilmAlternativeTitles.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmVideos.RemoveRange(await _db.FilmVideos.Where(x => x.FilmId == filmId).ToListAsync());
        _db.FilmReleaseDates.RemoveRange(await _db.FilmReleaseDates.Where(x => x.FilmId == filmId).ToListAsync());
    }

    private async Task<Collection?> ResolveCollectionAsync(Movie movie)
    {
        var info = movie.BelongsToCollection;
        if (info == null)
            return null;

        var collection = await _db.Collections.FirstOrDefaultAsync(c => c.TmdbId == info.Id);
        if (collection == null)
        {
            collection = new Collection
            {
                TmdbId = info.Id,
                Name = info.Name ?? string.Empty,
                PosterPath = info.PosterPath,
                BackdropPath = info.BackdropPath
            };
        }

        return collection;
    }

    private async Task SyncGenresAsync(Film film, Movie movie)
    {
        if (movie.Genres == null)
            return;

        List<int> ids = movie.Genres.Select(g => g.Id).Distinct().ToList();
        Dictionary<int, Genre> existing = await _db.Genres
            .Where(x => ids.Contains(x.TmdbId))
            .ToDictionaryAsync(x => x.TmdbId);

        foreach (var g in movie.Genres)
        {
            if (!existing.TryGetValue(g.Id, out var genre))
            {
                genre = new Genre { TmdbId = g.Id, Name = g.Name ?? string.Empty };
                existing[g.Id] = genre;
            }

            _db.FilmGenres.Add(new FilmGenre { Film = film, Genre = genre });
        }
    }

    private async Task SyncKeywordsAsync(Film film, Movie movie)
    {
        var keywords = movie.Keywords?.Keywords;
        if (keywords == null)
            return;

        List<int> ids = keywords.Select(k => k.Id).Distinct().ToList();
        Dictionary<int, Keyword> existing = await _db.Keywords
            .Where(x => ids.Contains(x.TmdbId))
            .ToDictionaryAsync(x => x.TmdbId);

        foreach (var k in keywords)
        {
            if (!existing.TryGetValue(k.Id, out var keyword))
            {
                keyword = new Keyword { TmdbId = k.Id, Name = k.Name ?? string.Empty };
                existing[k.Id] = keyword;
            }

            _db.FilmKeywords.Add(new FilmKeyword { Film = film, Keyword = keyword });
        }
    }

    private async Task SyncProductionCompaniesAsync(Film film, Movie movie)
    {
        if (movie.ProductionCompanies == null)
            return;

        List<int> ids = movie.ProductionCompanies.Select(pc => pc.Id).Distinct().ToList();
        Dictionary<int, ProductionCompany> existing = await _db.ProductionCompanies
            .Where(x => ids.Contains(x.TmdbId))
            .ToDictionaryAsync(x => x.TmdbId);

        int order = 0;
        foreach (var pc in movie.ProductionCompanies)
        {
            if (!existing.TryGetValue(pc.Id, out var company))
            {
                company = new ProductionCompany
                {
                    TmdbId = pc.Id,
                    Name = pc.Name ?? string.Empty,
                    LogoPath = pc.LogoPath,
                    OriginCountry = pc.OriginCountry
                };
                existing[pc.Id] = company;
            }

            _db.FilmProductionCompanies.Add(new FilmProductionCompany
            {
                Film = film,
                ProductionCompany = company,
                DisplayOrder = order++
            });
        }
    }

    private async Task SyncCreditsAsync(Film film, Movie movie)
    {
        if (movie.Credits == null)
            return;

        IEnumerable<int> castIds = movie.Credits.Cast?.Select(c => c.Id) ?? Enumerable.Empty<int>();
        IEnumerable<int> crewIds = movie.Credits.Crew?.Select(c => c.Id) ?? Enumerable.Empty<int>();
        List<int> ids = castIds.Concat(crewIds).Distinct().ToList();

        Dictionary<int, Person> personCache = await _db.People
            .Where(p => ids.Contains(p.TmdbId))
            .ToDictionaryAsync(p => p.TmdbId);

        if (movie.Credits.Cast != null)
        {
            foreach (var c in movie.Credits.Cast)
            {
                var person = ResolvePerson(personCache, c.Id, c.Name, (int)c.Gender, c.KnownForDepartment, c.ProfilePath, c.Popularity);

                _db.FilmCredits.Add(new FilmCredit
                {
                    Film = film,
                    Person = person,
                    CreditType = CreditTypeEnum.Cast,
                    Character = c.Character,
                    CreditOrder = c.Order,
                    TmdbCreditId = c.CreditId ?? string.Empty
                });
            }
        }

        if (movie.Credits.Crew != null)
        {
            foreach (var c in movie.Credits.Crew)
            {
                var person = ResolvePerson(personCache, c.Id, c.Name, (int)c.Gender, c.KnownForDepartment, c.ProfilePath, c.Popularity);

                _db.FilmCredits.Add(new FilmCredit
                {
                    Film = film,
                    Person = person,
                    CreditType = CreditTypeEnum.Crew,
                    Department = c.Department,
                    Job = c.Job,
                    TmdbCreditId = c.CreditId ?? string.Empty
                });
            }
        }
    }

    private static Person ResolvePerson(
        Dictionary<int, Person> personCache,
        int tmdbId,
        string? name,
        int genderCode,
        string? knownForDepartment,
        string? profilePath,
        double? popularity)
    {
        var utc = DateTime.UtcNow;

        if (personCache.TryGetValue(tmdbId, out var person))
        {
            person.Name = name ?? person.Name;
            person.Gender = (PersonGenderEnum)genderCode;
            person.KnownForDepartment = knownForDepartment ?? person.KnownForDepartment;
            person.ProfilePath = profilePath ?? person.ProfilePath;
            person.Popularity = popularity ?? person.Popularity;
            person.UpdatedAt = utc;
            return person;
        }

        person = new Person
        {
            TmdbId = tmdbId,
            Name = name ?? string.Empty,
            Gender = (PersonGenderEnum)genderCode,
            KnownForDepartment = knownForDepartment,
            ProfilePath = profilePath,
            Popularity = popularity,
            CreatedAt = utc,
            UpdatedAt = utc
        };

        personCache[tmdbId] = person;
        return person;
    }

    private static void SyncAlternativeTitles(Film film, Movie movie)
    {
        var titles = movie.AlternativeTitles?.Titles;
        if (titles == null)
            return;

        foreach (var t in titles)
        {
            film.AlternativeTitles.Add(new FilmAlternativeTitle
            {
                Film = film,
                CountryCode = t.Iso_3166_1,
                Title = t.Title ?? string.Empty,
                Type = string.IsNullOrWhiteSpace(t.Type) ? null : t.Type
            });
        }
    }

    private static void SyncVideos(Film film, Movie movie)
    {
        var videos = movie.Videos?.Results;
        if (videos == null)
            return;

        foreach (var v in videos)
        {
            film.Videos.Add(new FilmVideo
            {
                Film = film,
                Name = v.Name ?? string.Empty,
                Site = MapVideoSite(v.Site),
                Key = v.Key ?? string.Empty,
                VideoType = MapVideoType(v.Type),
                Official = v.Official,
                PublishedAt = v.PublishedAt
            });
        }
    }

    private static void SyncReleaseDates(Film film, Movie movie)
    {
        var results = movie.ReleaseDates?.Results;
        if (results == null)
            return;

        foreach (var country in results)
        {
            if (country.ReleaseDates == null)
                continue;

            foreach (var rd in country.ReleaseDates)
            {
                film.ReleaseDates.Add(new FilmReleaseDate
                {
                    Film = film,
                    CountryCode = country.Iso_3166_1 ?? string.Empty,
                    ReleaseDate = rd.ReleaseDate,
                    ReleaseType = (ReleaseDateTypeEnum)(int)rd.Type,
                    Certification = string.IsNullOrWhiteSpace(rd.Certification) ? null : rd.Certification,
                    Note = string.IsNullOrWhiteSpace(rd.Note) ? null : rd.Note
                });
            }
        }
    }

    private static FilmStatusEnum? MapStatus(string? status) => status switch
    {
        "Rumored" => FilmStatusEnum.Rumored,
        "Planned" => FilmStatusEnum.Planned,
        "In Production" => FilmStatusEnum.InProduction,
        "Post Production" => FilmStatusEnum.PostProduction,
        "Released" => FilmStatusEnum.Released,
        "Canceled" => FilmStatusEnum.Canceled,
        _ => null
    };

    private static VideoSiteEnum MapVideoSite(string? site) => site switch
    {
        "Vimeo" => VideoSiteEnum.Vimeo,
        _ => VideoSiteEnum.YouTube
    };

    private static VideoTypeEnum MapVideoType(string? type) => type switch
    {
        "Teaser" => VideoTypeEnum.Teaser,
        "Clip" => VideoTypeEnum.Clip,
        "Featurette" => VideoTypeEnum.Featurette,
        "Behind the Scenes" => VideoTypeEnum.BehindTheScenes,
        "Bloopers" => VideoTypeEnum.Bloopers,
        "Opening Credits" => VideoTypeEnum.OpeningCredits,
        _ => VideoTypeEnum.Trailer
    };
}
