using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class Person
{
    public int Id { get; set; }

    public int TmdbId { get; set; }

    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public PersonGenderEnum Gender { get; set; }

    [MaxLength(100)]
    public string? KnownForDepartment { get; set; }

    [MaxLength(500)]
    public string? ProfilePath { get; set; }

    [MaxLength(4000)]
    public string? Biography { get; set; }

    public DateTime? Birthday { get; set; }

    public DateTime? Deathday { get; set; }

    [MaxLength(500)]
    public string? PlaceOfBirth { get; set; }

    [MaxLength(20)]
    public string? ImdbId { get; set; }

    public double? Popularity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<FilmCredit> Credits { get; set; } = new List<FilmCredit>();
}
