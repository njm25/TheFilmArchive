namespace Api.Responses;

public class GetWatchProgressRes
{
    public required int ProgressSeconds { get; set; }
    public required int DurationSeconds { get; set; }
    public int? SourceId { get; set; }
}
