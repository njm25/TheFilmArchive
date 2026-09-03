namespace Domain.Entities;

public class WatchProgress
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int FilmId { get; set; }

    public int SourceId { get; set; }

    public int ProgressSeconds { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Film Film { get; set; } = null!;
}
