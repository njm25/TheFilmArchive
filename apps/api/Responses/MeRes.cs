using Infrastructure.Data;

namespace Api.Responses;

public class MeRes
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public RoleEnum Role { get; set; } = RoleEnum.User;
}
