namespace Api.Responses;

public class GetFilmsRes
{
    public required List<GetFilmResItem> Films { get; set; }
}

public class GetFilmResItem
{ 
    public int FilmId { get; set; }
    public required string Title { get; set; }
    public required int YearReleased { get; set; }
    public required string Description { get; set; }
    public required string PosterUrl { get; set; }
}
