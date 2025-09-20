using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RupalStudentCore8App.Server.Entities;

[Index("NormalizedName", Name = "UX_AspNetRoles_NormalizedName", IsUnique = true)]
public partial class AspNetRole : IdentityRole<int>
{

    //[Key]
    //public int Id { get; set; }
    //[Required]
    //[StringLength(256)]
    //public string Name { get; set; }
    //[Required]
    //[StringLength(256)]
    //public string NormalizedName { get; set; }
    //[Required]
    //public string ConcurrencyStamp { get; set; }: IdentityRole<int>

    public bool ReadOnly { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<AspMenuPermission> AspMenuPermissions { get; set; } = new List<AspMenuPermission>();

    [InverseProperty("Role")]
    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    [ForeignKey("RoleId")]
    [InverseProperty("Roles")]
    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
