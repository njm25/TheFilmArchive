using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Film
{
    public int Id { get; set; }

    public string TmdbId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int? ReleaseYear { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(4000)]
    public string? Tagline { get; set; }

    [MaxLength(500)]
    public string? PosterPath { get; set; }

    [MaxLength(500)]
    public string? BackdropPath { get; set; }

    public int? Runtime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<FilmSource> Sources { get; set; } = new List<FilmSource>();
}