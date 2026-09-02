namespace Api.Requests;

public class WatchProgressReq
{
    public required int FilmId { get; set; }
    public required int SourceId { get; set; }
    public required int ProgressSeconds { get; set; }
    public required int DurationSeconds { get; set; }
}
