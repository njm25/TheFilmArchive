namespace Domain.Entities;

public class FilmView
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Film Film { get; set; } = null!;
}
