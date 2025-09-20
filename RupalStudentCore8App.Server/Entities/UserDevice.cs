using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RupalStudentCore8App.Server.Entities;

[Index("UserId", "DeviceIdentifier", Name = "UX_UserDevices_UserId_DeviceIdentifier", IsUnique = true)]
    public partial class UserDevice
    {
        //public UserDevice()
        //{
            //RefreshTokens = new HashSet<AspNetRefreshToken>();
            //LastLogin = DateTime.UtcNow;
            //IsActive = true;
        //}
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

        /// <summary>
        /// Unique identifier for the device (e.g., hardware ID, browser fingerprint, UUID)
        /// This is NOT a foreign key, but rather a string that uniquely identifies the physical/virtual device
        /// </summary>
       
    [StringLength(256)]
    public string? DeviceIdentifier { get; set; }

        /// <summary>
        /// User-friendly name for the device
        /// </summary>
      
    [StringLength(100)]
    public string? DeviceName { get; set; }

        /// <summary>
        /// Type of device (e.g., Mobile, Desktop, Tablet)
        /// </summary>
       
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
