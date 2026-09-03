namespace Api.Responses;

public class GetLanguagesRes
{
    public required List<GetLanguageResItem> Languages { get; set; }
}

public class GetLanguageResItem
{
    public required string Code { get; set; }
    public required string Name { get; set; }
}
