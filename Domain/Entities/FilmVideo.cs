using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class FilmVideo
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public VideoSiteEnum Site { get; set; }

    [MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    public VideoTypeEnum VideoType { get; set; }

    public bool Official { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Film Film { get; set; } = null!;
}
