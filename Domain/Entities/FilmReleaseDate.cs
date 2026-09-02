using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class FilmReleaseDate
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public ReleaseDateTypeEnum ReleaseType { get; set; }

    [MaxLength(20)]
    public string? Certification { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public Film Film { get; set; } = null!;
}
