using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class AccountRequest
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public DateTime CreatedUtc { get; set; }
}