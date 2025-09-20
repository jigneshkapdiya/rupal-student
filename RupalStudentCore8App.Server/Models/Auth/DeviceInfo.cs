namespace RupalStudentCore8App.Server.Models.Auth
{
    public class DeviceInfo
    {
        /// <summary>
        /// Unique identifier for the device
        /// </summary>
        public string DeviceIdentifier { get; set; }

        /// <summary>
        /// Human-readable name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Type of device (Mobile, Tablet, Desktop, TV, etc.)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Operating system information
        /// </summary>
        public string OS { get; set; }

        /// <summary>
        /// Browser information
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// IP address of the device
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Full user agent string
        /// </summary>
        public string UserAgent { get; set; }
    }
}
