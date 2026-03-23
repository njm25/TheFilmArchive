using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class FilmSource
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    public SourceTypeEnum Type { get; set; }

    [MaxLength(2000)]
    public string SourceUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Film Film { get; set; } = null!;
}