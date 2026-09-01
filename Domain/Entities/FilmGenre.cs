namespace Domain.Entities;

public class FilmGenre
{
    public int FilmId { get; set; }

    public int GenreId { get; set; }

    public Film Film { get; set; } = null!;

    public Genre Genre { get; set; } = null!;
}
