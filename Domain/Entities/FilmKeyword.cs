namespace Domain.Entities;

public class FilmKeyword
{
    public int FilmId { get; set; }

    public int KeywordId { get; set; }

    public Film Film { get; set; } = null!;

    public Keyword Keyword { get; set; } = null!;
}
