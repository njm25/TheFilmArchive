namespace Api.Requests;

public class RegisterReq
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Token { get; set; }
}