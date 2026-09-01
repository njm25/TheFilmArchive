using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductionCompany
{
    public int Id { get; set; }

    public int TmdbId { get; set; }

    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoPath { get; set; }

    [MaxLength(2)]
    public string? OriginCountry { get; set; }

    public ICollection<FilmProductionCompany> Films { get; set; } = new List<FilmProductionCompany>();
}
