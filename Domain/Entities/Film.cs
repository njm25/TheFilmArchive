using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class Film
{
    public int Id { get; set; }

    public string TmdbId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ImdbId { get; set; }

    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? OriginalTitle { get; set; }

    [MaxLength(10)]
    public string? OriginalLanguage { get; set; }

    public int? ReleaseYear { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(4000)]
    public string? Tagline { get; set; }

    [MaxLength(500)]
    public string? PosterPath { get; set; }

    [MaxLength(500)]
    public string? BackdropPath { get; set; }

    [MaxLength(500)]
    public string? Homepage { get; set; }

    public int? Runtime { get; set; }

    public FilmStatusEnum? Status { get; set; }

    public bool Adult { get; set; }

    public long? Budget { get; set; }

    public long? Revenue { get; set; }

    public double? Popularity { get; set; }

    public double? VoteAverage { get; set; }

    public int? VoteCount { get; set; }

    public int? CollectionId { get; set; }

    public Collection? Collection { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<FilmSource> Sources { get; set; } = new List<FilmSource>();

    public ICollection<FilmCredit> Credits { get; set; } = new List<FilmCredit>();

    public ICollection<FilmGenre> Genres { get; set; } = new List<FilmGenre>();

    public ICollection<FilmKeyword> Keywords { get; set; } = new List<FilmKeyword>();

    public ICollection<FilmProductionCompany> ProductionCompanies { get; set; } = new List<FilmProductionCompany>();

    public ICollection<FilmAlternativeTitle> AlternativeTitles { get; set; } = new List<FilmAlternativeTitle>();

    public ICollection<FilmVideo> Videos { get; set; } = new List<FilmVideo>();

    public ICollection<FilmReleaseDate> ReleaseDates { get; set; } = new List<FilmReleaseDate>();

    public ICollection<FilmView> Views { get; set; } = new List<FilmView>();
}
