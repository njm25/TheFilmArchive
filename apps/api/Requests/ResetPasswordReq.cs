namespace Api.Requests;

public class ResetPasswordReq
{
    public required string Token { get; set; }
    public required string Password { get; set; }
}
