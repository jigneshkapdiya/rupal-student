using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RupalStudentCore8App.Server.Entities;

[Table("AspMenu")]
[Index("Name", Name = "UX_AspMenu_Name", IsUnique = true)]
public partial class AspMenu
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public string? Title { get; set; }

    public string? TitleAr { get; set; }

    [StringLength(100)]
    public string? Icon { get; set; }

    [StringLength(4000)]
    public string? Path { get; set; }

    public int? ParentId { get; set; }

    public int OrderNo { get; set; }

    [StringLength(50)]
    public string? Class { get; set; }

    public bool IsExternalLink { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<AspMenuPermission> AspMenuPermissions { get; set; } = new List<AspMenuPermission>();

    [InverseProperty("Menu")]
    public virtual ICollection<AspNetPermission> AspNetPermissions { get; set; } = new List<AspNetPermission>();

    [InverseProperty("Parent")]
    public virtual ICollection<AspMenu> InverseParent { get; set; } = new List<AspMenu>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual AspMenu? Parent { get; set; }
}
