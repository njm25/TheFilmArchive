using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class FilmCredit
{
    public int Id { get; set; }

    public int FilmId { get; set; }

    public int PersonId { get; set; }

    public CreditTypeEnum CreditType { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(200)]
    public string? Job { get; set; }

    [MaxLength(300)]
    public string? Character { get; set; }

    public int? CreditOrder { get; set; }

    [MaxLength(64)]
    public string TmdbCreditId { get; set; } = string.Empty;

    public Film Film { get; set; } = null!;

    public Person Person { get; set; } = null!;
}
