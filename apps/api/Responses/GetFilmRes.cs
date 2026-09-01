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
    public string? ImdbId { get; set; }
    public string? Homepage { get; set; }
    public FilmStatusEnum? Status { get; set; }
    public double? VoteAverage { get; set; }
    public int? VoteCount { get; set; }
    public string? CollectionName { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
    public List<string> Directors { get; set; } = new List<string>();
    public List<GetFilmResCastMember> Cast { get; set; } = new List<GetFilmResCastMember>();
}

public class GetFilmResCastMember
{
    public required string Name { get; set; }
    public string? Character { get; set; }
    public string? ProfilePath { get; set; }
}

public class GetFilmResSource
{
    public int SourceId { get; set; }

    public SourceTypeEnum Type { get; set; }
}
