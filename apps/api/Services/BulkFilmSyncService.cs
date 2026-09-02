using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Clients;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TMDbLib.Objects.Movies;

namespace Api.Services;

/// <summary>
/// Runs the full admin-triggered bulk operation: import any films from the
/// checked-in seed manifest that aren't in the database yet, then refresh
/// TMDB metadata for every film already in the database. Always creates its
/// own DI scope - the caller (a fire-and-forget Task.Run from a controller)
/// must never hand this a request-scoped AppDbContext/TmdbService directly,
/// since those are disposed the moment the HTTP response is sent.
/// </summary>
public class BulkFilmSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BulkSyncJobService _jobService;

    public BulkFilmSyncService(IServiceScopeFactory scopeFactory, BulkSyncJobService jobService)
    {
        _scopeFactory = scopeFactory;
        _jobService = jobService;
    }

    public async Task RunAsync()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        TmdbService tmdb = scope.ServiceProvider.GetRequiredService<TmdbService>();
        FilmSyncService filmSync = scope.ServiceProvider.GetRequiredService<FilmSyncService>();
        ArchiveOrgService archiveOrg = scope.ServiceProvider.GetRequiredService<ArchiveOrgService>();

        List<SeedFilmEntry> seeds = LoadSeedManifest();
        _jobService.SetTotal(seeds.Count);
        _jobService.SetPhase("Importing seed films");

        HashSet<string> createdTmdbIds = new();

        // TmdbIds for every seed entry we successfully resolved, whether newly
        // created or already present. Used below to size the refresh phase
        // without re-counting films the import phase already accounted for.
        HashSet<string> matchedSeedTmdbIds = new();

        foreach (SeedFilmEntry seed in seeds)
        {
            _jobService.SetCurrent($"Importing: {seed.Title}");

            // A film that already exists gets its real unit of work done in the
            // refresh phase below, not here - so it shouldn't also mark itself
            // processed in this phase (that would double-count it against Total).
            bool deferToRefreshPhase = false;

            try
            {
                string? tmdbId = string.IsNullOrWhiteSpace(seed.TmdbId)
                    ? await tmdb.FindTmdbIdByImdbId(seed.ImdbId)
                    : seed.TmdbId;

                if (string.IsNullOrWhiteSpace(tmdbId))
                {
                    _jobService.RecordFailure(seed.Title, $"Could not resolve a TMDB ID from IMDb ID {seed.ImdbId}");
                    continue;
                }

                matchedSeedTmdbIds.Add(tmdbId);

                bool exists = await db.Films.AnyAsync(f => f.TmdbId == tmdbId);
                if (exists)
                {
                    _jobService.IncrementSkipped();
                    deferToRefreshPhase = true;
                    continue;
                }

                Movie? movie = await tmdb.GetMovieByTmdbId(tmdbId);
                if (movie == null)
                {
                    _jobService.RecordFailure(seed.Title, $"TMDB returned no movie for id {tmdbId}");
                    continue;
                }

                DateTime utc = DateTime.UtcNow;
                Film film = new Film
                {
                    TmdbId = tmdbId,
                    CreatedAt = utc,
                    UpdatedAt = utc
                };

                db.Films.Add(film);

                await filmSync.ApplyMetadataAsync(film, movie);

                bool isPrimary = true;
                foreach (string url in seed.ArchiveUrls)
                {
                    int? quality = null;
                    string? identifier = ArchiveOrgService.ExtractIdentifier(url);

                    if (identifier != null)
                    {
                        quality = await archiveOrg.GetBestQualityHeightAsync(identifier);
                    }

                    db.FilmSources.Add(new FilmSource
                    {
                        Film = film,
                        SourceUrl = url,
                        Type = SourceTypeEnum.ArchiveOrg,
                        QualityHeight = quality,
                        IsPrimary = isPrimary,
                        IsActive = true,
                        CreatedAt = utc
                    });

                    isPrimary = false;
                }

                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();

                createdTmdbIds.Add(tmdbId);
                _jobService.IncrementCreated();
            }
            catch (Exception ex)
            {
                _jobService.RecordFailure(seed.Title, ex.Message);
            }
            finally
            {
                if (!deferToRefreshPhase)
                    _jobService.IncrementProcessed();
            }
        }

        List<Film> existingFilms = await db.Films
            .Where(f => !createdTmdbIds.Contains(f.TmdbId))
            .ToListAsync();

        // Only films with no seed-manifest counterpart are "new" to the total -
        // the rest were already counted once via seeds.Count above and are just
        // now getting their refresh pass, not a second film slot.
        int extraFilmsCount = existingFilms.Count(f => !matchedSeedTmdbIds.Contains(f.TmdbId));
        _jobService.IncreaseTotalBy(extraFilmsCount);
        _jobService.SetPhase("Refreshing existing films");

        foreach (Film film in existingFilms)
        {
            _jobService.SetCurrent($"Refreshing: {film.Title}");

            try
            {
                Movie? movie = await tmdb.GetMovieByTmdbId(film.TmdbId);

                if (movie == null)
                {
                    _jobService.RecordFailure(film.Title, "TMDB returned no movie for this film's TmdbId");
                    continue;
                }

                // A prior iteration's ChangeTracker.Clear() (see below) leaves this
                // film detached even though it's a real, already-saved row - without
                // re-attaching it first, EF's Add() of its new child rows below would
                // walk the graph and try to re-insert the film itself as a new row.
                // Must happen before ApplyMetadataAsync mutates its scalar fields, so
                // EF's change-detection baseline reflects the pre-refresh DB values.
                db.Attach(film);

                await filmSync.ApplyMetadataAsync(film, movie);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();

                _jobService.IncrementRefreshed();
            }
            catch (Exception ex)
            {
                _jobService.RecordFailure(film.Title, ex.Message);
            }
            finally
            {
                _jobService.IncrementProcessed();
            }
        }

        _jobService.Complete();
    }

    private static List<SeedFilmEntry> LoadSeedManifest()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Storage", "seed-films.json");

        if (!File.Exists(path))
            return new List<SeedFilmEntry>();

        string json = File.ReadAllText(path);

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<SeedFilmEntry>>(json, options) ?? new List<SeedFilmEntry>();
    }
}
