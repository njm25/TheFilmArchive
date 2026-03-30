namespace Api.Requests;

public class GetFilmsReq : GenericListReq
{
    public required OrderFilmByEnum OrderBy { get; set; }
}

public enum OrderFilmByEnum
{
    YearReleased = 1,
}
