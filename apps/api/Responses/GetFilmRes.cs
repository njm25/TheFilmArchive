using Domain.Enums;

namespace Api.Responses;

public class GetFilmRes
{
    public required string Title { get; set; }
    public required int YearReleased { get; set; }
    public required string Description { get; set; }
    public required string Tagline { get; set; }
    public string? PosterPath { get; set; }
    public required List<GetFilmResSource> Sources { get; set; } = new List<GetFilmResSource>();
    public required int PrimarySourceTypeId { get; set; }
    public string? BackdropPath { get; set; }
    public required int Runtime { get; set; }
    public string? ImdbId { get; set; }
    public string? Homepage { get; set; }
    public FilmStatusEnum? Status { get; set; }
    public double? VoteAverage { get; set; }
    public int? VoteCount { get; set; }
    public string? CollectionName { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
    public List<GetFilmResPerson> Directors { get; set; } = new List<GetFilmResPerson>();
    public List<GetFilmResCastMember> Cast { get; set; } = new List<GetFilmResCastMember>();
}

// Carries the person id alongside the name so the film page can link a credit
// through to that person's filmography.
public class GetFilmResPerson
{
    public required int PersonId { get; set; }
    public required string Name { get; set; }
}

public class GetFilmResCastMember
{
    public required int PersonId { get; set; }
    public required string Name { get; set; }
    public string? Character { get; set; }
    public string? ProfilePath { get; set; }
}

public class GetFilmResSource
{
    public int SourceId { get; set; }

    public SourceTypeEnum Type { get; set; }

    public int? QualityHeight { get; set; }
}
