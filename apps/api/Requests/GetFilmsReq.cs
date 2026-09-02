namespace Api.Requests;

public class GetFilmsReq : GenericListReq
{
    public required OrderFilmByEnum OrderBy { get; set; }
    public List<int>? GenreIds { get; set; }
    public double? MinRating { get; set; }
}

public enum OrderFilmByEnum
{
    YearReleased = 1,
    Rating = 2,
    Title = 3,
    CreatedAt = 4
}
