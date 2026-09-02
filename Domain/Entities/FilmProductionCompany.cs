namespace Domain.Entities;

public class FilmProductionCompany
{
    public int FilmId { get; set; }

    public int ProductionCompanyId { get; set; }

    public int DisplayOrder { get; set; }

    public Film Film { get; set; } = null!;

    public ProductionCompany ProductionCompany { get; set; } = null!;
}
