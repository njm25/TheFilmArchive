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

    /// <summary>
    /// Vertical resolution in pixels (e.g. 480, 720, 1080), when known.
    /// Auto-detected for archive.org sources via their metadata API;
    /// set manually for S3 sources since we don't inspect the file.
    /// </summary>
    public int? QualityHeight { get; set; }

    public DateTime CreatedAt { get; set; }

    public Film Film { get; set; } = null!;
}