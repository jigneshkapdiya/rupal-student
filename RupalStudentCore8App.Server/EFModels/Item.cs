using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Table("Item")]
public partial class Item
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Barcode { get; set; } = null!;

    [StringLength(100)]
    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string Unit { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Vat { get; set; }

    [StringLength(500)]
    public string? ImagePath { get; set; }
}
