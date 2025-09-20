using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Table("LoginHistory")]
public partial class LoginHistory
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LogInTime { get; set; }

    public bool? IsSuccessful { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(500)]
    public string? Device { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("LoginHistories")]
    public virtual AspNetUser User { get; set; } = null!;
}
