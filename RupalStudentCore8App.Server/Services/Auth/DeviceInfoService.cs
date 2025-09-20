using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UAParser;

namespace RupalStudentCore8App.Server.Services.Auth
{
    public interface IDeviceInfoService
    {
        DeviceInfo GetDeviceInfo();
        string GetDeviceFingerprint();
        string GetDeviceName(HttpContext context);
        Task<List<UserDevice>> GetUserDevices(int userId);
        Task RemoveDevice(int userId, int deviceId);
    }

    public class DeviceInfoService : IDeviceInfoService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Parser _uaParser;

        public DeviceInfoService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _uaParser = Parser.GetDefault();
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public DeviceInfo GetDeviceInfo()
        {
            var context = _httpContextAccessor.HttpContext;

            // Get User Agent
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var clientInfo = _uaParser.Parse(userAgent);

            // Get Device ID from headers or generate a fallback
            var deviceId = GetSanitizedDeviceId(context.Request.Headers["Device-Id"].FirstOrDefault());

            // Get IP Address
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            if (ip == "::1") ip = "127.0.0.1"; // Convert localhost IPv6 to IPv4

            return new DeviceInfo
            {
                DeviceIdentifier = deviceId,
                DeviceName = GetSanitizedValue(context.Request.Headers["Device-Name"].FirstOrDefault())
                    ?? $"{clientInfo.Device.Brand} {clientInfo.Device.Model}".Trim(),
                DeviceType = GetSanitizedValue(context.Request.Headers["Device-Type"].FirstOrDefault())
                    ?? DetermineDeviceType(clientInfo),
                OS = GetSanitizedValue(context.Request.Headers["Device-OS"].FirstOrDefault())
                    ?? $"{clientInfo.OS.Family} {clientInfo.OS.Major}".Trim(),
                Browser = GetSanitizedValue(context.Request.Headers["Device-Browser"].FirstOrDefault())
                    ?? $"{clientInfo.UA.Family} {clientInfo.UA.Major}".Trim(),
                IpAddress = ip,
                UserAgent = userAgent
            };
        }

        private string GetSanitizedDeviceId(string deviceId)
        {
            //if (string.IsNullOrWhiteSpace(deviceId))
            //    throw new ArgumentException("Device-Id header is required");

            if (string.IsNullOrWhiteSpace(deviceId))
                deviceId = GetDeviceFingerprint();

            // Remove any non-alphanumeric characters except dashes and underscores
            deviceId = Regex.Replace(deviceId, "[^a-zA-Z0-9\\-_]", "");

            if (deviceId.Length < 8 || deviceId.Length > 64)
                throw new ArgumentException("Device-Id must be between 8 and 64 characters");

            return deviceId;
        }

        private string? GetSanitizedValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Remove any potentially dangerous characters
            value = Regex.Replace(value, "[^a-zA-Z0-9\\-_. ]", "");
            return value.Length > 100 ? value[..100] : value;
        }

        private string DetermineDeviceType(ClientInfo clientInfo)
        {
            var device = clientInfo.Device.Family.ToLower();
            var os = clientInfo.OS.Family.ToLower();

            if (device.Contains("mobile") || os.Contains("android") || os.Contains("ios"))
                return "Mobile";
            if (device.Contains("tablet") || os.Contains("ipad"))
                return "Tablet";
            if (device.Contains("tv") || os.Contains("tv"))
                return "TV";
            return "Desktop";
        }

        public string GetDeviceFingerprint()
        {
            var context = _httpContextAccessor.HttpContext;
            var ua = context.Request.Headers["User-Agent"].ToString();
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var combined = $"{ua}-{ip}";

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash);
        }

        public string GetDeviceName(HttpContext context)
        {
            var ua = context.Request.Headers["User-Agent"].ToString();
            // You can use a library like UAParser to extract more details
            return ua.Length > 50 ? ua.Substring(0, 50) : ua;
        }

        public async Task<List<UserDevice>> GetUserDevices(int userId)
        {
            return await _dbContext.UserDevices
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.LastLogin)
                .ToListAsync();
        }

        public async Task RemoveDevice(int userId, int deviceId)
        {
            var device = await _dbContext.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deviceId);

            if (device != null)
            {
                device.IsActive = false;
                device.IsRevoked = true;
                device.RevokedDate = DateTime.UtcNow;
                _dbContext.UserDevices.Update(device);
                // Optionally, remove associated refresh tokens
                var refreshTokens = await _dbContext.AspNetRefreshTokens
                    .Where(rt => rt.UserId==userId && rt.DeviceIdentifier == device.DeviceIdentifier)
                    .ToListAsync();
                if (refreshTokens.Any())
                {
                    foreach (var token in refreshTokens)
                    {

                        token.RevokedOn = DateTime.UtcNow;
                        token.RevokedReason = "Removed device by user";
                        // token.IsActive = false;
                        //token.IsRevoked = true;

                    }
                    _dbContext.AspNetRefreshTokens.UpdateRange(refreshTokens);
                    await _dbContext.SaveChangesAsync();
                }

            }
        }
    }
}
