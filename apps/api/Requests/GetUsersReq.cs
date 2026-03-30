namespace Api.Requests;

public class GetUsersReq : GenericListReq
{
    public required OrderUserByEnum OrderBy { get; set; }
}

public enum OrderUserByEnum
{
    Id = 1,
    UserName = 2,
    Email = 3,
}
