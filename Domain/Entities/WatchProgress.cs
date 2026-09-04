namespace Domain.Entities;

public class WatchProgress
{
    // Nobody watches a film to the last frame - credits roll, archive.org
    // transfers have dead air on the end, and players stop a beat short. So a
    // film counts as finished once the viewer is inside this allowance of the
    // end, sized as a share of the runtime and clamped at both ends: a feature
    // shouldn't need ten minutes of credits to qualify, and a two-minute short
    // shouldn't be called finished a third of the way in.
    private const double EndAllowanceFraction = 0.10;
    private const int MinEndAllowanceSeconds = 15;
    private const int MaxEndAllowanceSeconds = 180;

    public int Id { get; set; }

    public required string UserId { get; set; }

    public int FilmId { get; set; }

    public int SourceId { get; set; }

    public int ProgressSeconds { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime UpdatedAt { get; set; }

    // When the viewer last reached the end. Null means unwatched or partway
    // through - a completed film that gets started again clears this, so it
    // returns to Continue Watching on its own.
    public DateTime? CompletedAt { get; set; }

    public Film Film { get; set; } = null!;

    public static bool IsComplete(int progressSeconds, int durationSeconds)
    {
        // Duration is only known once the player has reported it, so treat an
        // unknown runtime as "not finished" rather than guessing.
        if (durationSeconds <= 0)
            return false;

        int allowance = Math.Clamp(
            (int)(durationSeconds * EndAllowanceFraction),
            MinEndAllowanceSeconds,
            MaxEndAllowanceSeconds);

        return progressSeconds >= durationSeconds - allowance;
    }
}
