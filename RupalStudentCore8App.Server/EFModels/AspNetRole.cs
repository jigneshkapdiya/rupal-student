using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Index("NormalizedName", Name = "UX_AspNetRoles_NormalizedName", IsUnique = true)]
public partial class AspNetRole
{
    [Key]
    public int Id { get; set; }

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string NormalizedName { get; set; } = null!;

    public string ConcurrencyStamp { get; set; } = null!;

    public bool ReadOnly { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<AspMenuPermission> AspMenuPermissions { get; set; } = new List<AspMenuPermission>();

    [InverseProperty("Role")]
    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    [ForeignKey("RoleId")]
    [InverseProperty("Roles")]
    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
