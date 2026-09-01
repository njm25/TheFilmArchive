using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class FilmAlternativeTitle
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    [MaxLength(2)]
    public string? CountryCode { get; set; }

    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Type { get; set; }

    public Film Film { get; set; } = null!;
}
