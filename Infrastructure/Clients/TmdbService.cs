using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace Infrastructure.Clients;

public class TmdbService
{
    private const MovieMethods MetadataMethods =
        MovieMethods.Credits |
        MovieMethods.Keywords |
        MovieMethods.Videos |
        MovieMethods.ReleaseDates |
        MovieMethods.AlternativeTitles;

    private readonly TMDbClient _client;

    public TmdbService(string apiKey)
    {
        _client = new TMDbClient(apiKey);
    }

    public async Task<Movie?> GetMovieByTmdbId(string tmdbId)
    {
        if (!int.TryParse(tmdbId, out int id))
            return null;

        return await _client.GetMovieAsync(id, MetadataMethods);
    }
}
