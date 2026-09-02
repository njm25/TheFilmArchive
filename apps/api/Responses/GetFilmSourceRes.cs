using Domain.Enums;

namespace Api.Responses;

public class GetFilmSourceRes
{
    public SourceTypeEnum Type { get; set; }

    public required string Url { get; set; }
}
