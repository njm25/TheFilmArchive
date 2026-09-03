namespace Api.Requests;

public class GetFilmsReq : GenericListReq
{
    public required OrderFilmByEnum OrderBy { get; set; }
    public List<int>? GenreIds { get; set; }
    public double? MinRating { get; set; }
    public double? MaxRating { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MinRuntime { get; set; }
    public int? MaxRuntime { get; set; }
    public List<string>? Languages { get; set; }
}

public enum OrderFilmByEnum
{
    YearReleased = 1,
    Rating = 2,
    Title = 3,
    CreatedAt = 4
}
