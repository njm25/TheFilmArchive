namespace Api.Services;

public static class RateLimitPolicies
{
    // Shared by every endpoint that puts mail on the wire - sign-up and
    // password reset both cost money and both can be pointed at a stranger.
    public const string OutboundEmail = "outbound-email";
    public const string Login = "login";
}
