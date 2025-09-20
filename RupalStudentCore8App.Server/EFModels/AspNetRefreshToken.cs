using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Table("AspNetRefreshToken")]
[Index("Token", Name = "UX_AspNetRefreshToken_Token", IsUnique = true)]
public partial class AspNetRefreshToken
{
    [Key]
    public int Id { get; set; }

    [StringLength(256)]
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    [StringLength(256)]
    public string DeviceIdentifier { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpireOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RevokedOn { get; set; }

    [StringLength(200)]
    public string? RevokedReason { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AspNetRefreshTokens")]
    public virtual AspNetUser User { get; set; } = null!;
}
