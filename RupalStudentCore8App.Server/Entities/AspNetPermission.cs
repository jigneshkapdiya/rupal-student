using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RupalStudentCore8App.Server.Entities;

public partial class AspNetPermission
{
    [Key]
    public int Id { get; set; }

    public int MenuId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("AspNetPermissions")]
    public virtual AspMenu Menu { get; set; } = null!;
}
