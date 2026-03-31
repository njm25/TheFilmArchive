using Domain.Enums;

namespace Api.Responses;

public class GetFilmRes
{
    public required string Title { get; set; }
    public required int YearReleased { get; set; }
    public required string Description { get; set; }
    public required string Tagline { get; set; }
    public required string PosterPath { get; set; }
    public required List<GetFilmResSource> Sources { get; set; } = new List<GetFilmResSource>();
    public required int PrimarySourceTypeId { get; set; }
    public required string BackdropPath { get; set; }
    public required int Runtime { get; set; }

}

public class GetFilmResSource
{ 
    public int SourceId { get; set; }
 
    public SourceTypeEnum Type { get; set; }
}
