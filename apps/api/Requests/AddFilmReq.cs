using Domain.Enums;

namespace Api.Requests;

public class AddFilmReq
{
    public required string TmdbId { get; set; }
}
