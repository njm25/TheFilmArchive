using System;
using System.Collections.Generic;
using System.Text;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;
namespace Infrastructure.Clients;

public class TmdbService
{
    private TMDbClient _client;
    public TmdbService(string apiKey)
    {
        _client = new TMDbClient(apiKey);
    }

    public async Task<Movie?> GetMovieByTmdbId(string tmdbId)
        => await _client.GetMovieAsync(tmdbId);
     
}
