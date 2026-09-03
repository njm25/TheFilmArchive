namespace Api.Responses;

public class GetCaptionsRes
{
    public required List<GetCaptionResItem> Captions { get; set; }
}

public class GetCaptionResItem
{
    public required string Label { get; set; }
    public required string Url { get; set; }
}
