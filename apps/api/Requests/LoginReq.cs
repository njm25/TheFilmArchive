namespace Api.Requests;

public class LoginReq
{
    public required string UserNameOrEmail { get; set; }
    public required string Password { get; set; }
}