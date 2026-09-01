using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Genre
{
    public int Id { get; set; }

    public int TmdbId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<FilmGenre> Films { get; set; } = new List<FilmGenre>();
}
