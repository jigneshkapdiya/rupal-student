namespace RupalStudentCore8App.Server.Models.Auth;

/// <summary>
/// Represents an active user session with device and location information
/// </summary>
public class SessionInfo
{
    /// <summary>
    /// Unique identifier for the device
    /// </summary>tujme sach me 
    public string DeviceId { get; set; }

    /// <summary>
    /// Name of the device (e.g., "John's iPhone")
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// Type of device (e.g., "Mobile", "Desktop", "Tablet")
    /// </summary>
    public string DeviceType { get; set; }

    /// <summary>
    /// Browser used for the session (e.g., "Chrome", "Firefox")
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// Operating system of the device
    /// </summary>
    public string OS { get; set; }

    /// <summary>
    /// IP address of the device
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Timestamp of the last activity in this session
    /// </summary>
    public DateTime LastActivity { get; set; }

    /// <summary>
    /// Whether this session has a valid, non-revoked refresh token
    /// </summary>
    public bool HasActiveToken { get; set; }
}
