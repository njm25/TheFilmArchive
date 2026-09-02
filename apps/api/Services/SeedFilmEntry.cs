namespace Api.Services;

public class SeedFilmEntry
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string ImdbId { get; set; } = string.Empty;
    public string? TmdbId { get; set; }
    public List<string> ArchiveUrls { get; set; } = new();
}
