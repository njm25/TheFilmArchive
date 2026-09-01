using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Keyword
{
    public int Id { get; set; }

    public int TmdbId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<FilmKeyword> Films { get; set; } = new List<FilmKeyword>();
}
