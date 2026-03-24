namespace Api.Requests;

public class GetFilmsReq
{
    public required int PageSize { get; set; }
    public required int PageNumber { get; set; }
    public required string SearchText { get; set; }
    public required OrderByEnum OrderBy { get; set; }
    public required OrderingTypeEnum OrderingType { get; set; }
}

public enum OrderByEnum
{ 
    YearReleased = 1,
}

public enum OrderingTypeEnum
{
    Ascending = 0,
    Descending = 1
}