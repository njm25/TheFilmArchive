namespace Api.Responses;

public class GetAccountRequestsRes
{
    public List<GetAccountRequestsResItem> AccountRequests { get; set; } = new List<GetAccountRequestsResItem>();
}

public class GetAccountRequestsResItem
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}