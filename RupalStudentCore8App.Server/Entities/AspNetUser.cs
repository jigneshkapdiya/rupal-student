using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RupalStudentCore8App.Server.Entities;

[Index("NormalizedEmail", Name = "NormalizedEmail_UNIQUE", IsUnique = true)]
[Index("NormalizedUserName", Name = "NormalizedUserName_UNIQUE", IsUnique = true)]
public partial class AspNetUser : IdentityUser<int>
{
     

        //[Key]
        //public int Id { get; set; }
        //[StringLength(256)]
        //public string UserName { get; set; }
        //[StringLength(256)]
        //public string NormalizedUserName { get; set; }
        //[StringLength(256)]
        //public string Email { get; set; }
        //[StringLength(256)]
        //public string NormalizedEmail { get; set; }
        //public bool EmailConfirmed { get; set; }
        //public string PasswordHash { get; set; }
        //public string SecurityStamp { get; set; }
        //public string ConcurrencyStamp { get; set; }
        [StringLength(15)]
    	public string? PhoneCode { get; set; }
        //[StringLength(50)]
        //public string PhoneNumber { get; set; }
        //public bool PhoneNumberConfirmed { get; set; }
        //public bool TwoFactorEnabled { get; set; }
        //public DateTimeOffset? LockoutEnd { get; set; }
        //public bool LockoutEnabled { get; set; }
        //public int AccessFailedCount { get; set; }
		
		
    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(100)]
    public string? FullNameAr { get; set; }

    public bool Status { get; set; }

    public bool? Readonly { get; set; }

    [StringLength(50)]
    public string? AccessKey { get; set; }

    [StringLength(50)]
    public string? ProfileImage { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AspNetRefreshToken> AspNetRefreshTokens { get; set; } = new List<AspNetRefreshToken>();
        //[InverseProperty("User")]
        //public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        //[InverseProperty("User")]
        //public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        //[InverseProperty("User")]
        //public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; }
        
    [InverseProperty("User")]
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    [InverseProperty("User")]
    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
