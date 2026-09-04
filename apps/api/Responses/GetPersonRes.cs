namespace Api.Responses;

public class GetPersonRes
{
    public required int PersonId { get; set; }
    public required string Name { get; set; }
    public string? ProfilePath { get; set; }

    // Films this person directed, newest first.
    public List<GetPersonResFilm> Directed { get; set; } = new List<GetPersonResFilm>();

    // Films this person appeared in, newest first. A person who directed and
    // starred in the same film shows up in both lists.
    public List<GetPersonResFilm> Acted { get; set; } = new List<GetPersonResFilm>();
}

// Mirrors the browse-list item so the client can hand these straight to the
// same film-card grid; Character is the one extra thing a filmography carries.
public class GetPersonResFilm : GetFilmResItem
{
    public string? Character { get; set; }
}
