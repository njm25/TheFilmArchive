namespace Api.Responses;

public class GetGenresRes
{
    public required List<GetGenreResItem> Genres { get; set; }
}

public class GetGenreResItem
{
    public int GenreId { get; set; }
    public required string Name { get; set; }
}
