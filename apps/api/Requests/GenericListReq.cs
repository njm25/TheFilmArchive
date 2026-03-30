namespace Api.Requests;

public class GenericListReq
{
    public required int PageSize { get; set; }
    public required int PageNumber { get; set; }
    public string? SearchText { get; set; }
    public required OrderingTypeEnum OrderingType { get; set; }
}

public enum OrderingTypeEnum
{
    Ascending = 0,
    Descending = 1
}