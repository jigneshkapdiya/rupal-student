using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Table("Attachment")]
public partial class Attachment
{
    [Key]
    public int Id { get; set; }

    public int ReferenceId { get; set; }

    [StringLength(100)]
    public string? ReferenceType { get; set; }

    [StringLength(1000)]
    public string? FileName { get; set; }

    [StringLength(50)]
    public string? FileUrl { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
