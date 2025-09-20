using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Table("AutoIncrement")]
[Index("Entity", Name = "UX_AutoIncrement_Entity", IsUnique = true)]
public partial class AutoIncrement
{
    [Key]
    public int Id { get; set; }

    public int IncrementId { get; set; }

    [StringLength(10)]
    public string? Prefix { get; set; }

    public int PadNumber { get; set; }

    [StringLength(100)]
    public string? Entity { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
