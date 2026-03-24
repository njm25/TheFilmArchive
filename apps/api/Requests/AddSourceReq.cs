using Domain.Enums;

namespace Api.Requests;

public class AddSourceReq
{
    public required int FilmId { get; set; }
    public required SourceTypeEnum SourceType { get; set; }
    public required string SourceUrl { get; set; }
}
