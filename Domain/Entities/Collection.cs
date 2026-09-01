using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Collection
{
    public int Id { get; set; }

    public int TmdbId { get; set; }

    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Overview { get; set; }

    [MaxLength(500)]
    public string? PosterPath { get; set; }

    [MaxLength(500)]
    public string? BackdropPath { get; set; }

    public ICollection<Film> Films { get; set; } = new List<Film>();
}
