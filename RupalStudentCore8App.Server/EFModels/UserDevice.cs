using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

[Index("UserId", "DeviceIdentifier", Name = "UX_UserDevices_UserId_DeviceIdentifier", IsUnique = true)]
public partial class UserDevice
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(256)]
    public string? DeviceIdentifier { get; set; }

    [StringLength(100)]
    public string? DeviceName { get; set; }

    [StringLength(20)]
    public string? DeviceType { get; set; }

    [Column("OS")]
    [StringLength(100)]
    public string? Os { get; set; }

    [StringLength(200)]
    public string? Browser { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public DateTime FirstLogin { get; set; }

    public DateTime LastLogin { get; set; }

    public bool IsActive { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserDevices")]
    public virtual AspNetUser User { get; set; } = null!;
}
