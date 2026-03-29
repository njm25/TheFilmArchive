using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public RoleEnum Role { get; set; } = RoleEnum.User;
}

public enum RoleEnum
{
    [Description("User")]
    User = 0,
    [Description("Admin")]
    Admin = 1,
    [Description("System Admin")]
    SysAdmin = 99,
}
