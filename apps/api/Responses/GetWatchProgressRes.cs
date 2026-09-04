namespace Api.Responses;

public class GetWatchProgressRes
{
    public required int ProgressSeconds { get; set; }
    public required int DurationSeconds { get; set; }
    public int? SourceId { get; set; }

    // True once the viewer has reached the end. The client uses it to start a
    // rewatch from the beginning instead of resuming into the credits.
    public required bool Completed { get; set; }
}
