using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RupalStudentCore8App.Server.Entities;

public partial class AspMenuPermission
{
    [Key]
    public int Id { get; set; }

    public int? MenuId { get; set; }

    public int? RoleId { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("AspMenuPermissions")]
    public virtual AspMenu? Menu { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("AspMenuPermissions")]
    public virtual AspNetRole? Role { get; set; }
}
