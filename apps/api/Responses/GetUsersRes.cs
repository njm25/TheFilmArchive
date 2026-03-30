using Infrastructure.Data;

namespace Api.Responses;

public class GetUsersRes
{
    public List<GetUsersResItem> Users { get; set; } = new List<GetUsersResItem>();
}
public class GetUsersResItem
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required RoleEnum Role { get; set; }
}
