
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace RupalStudentCore8App.Server.Services
{
    public interface ILoginHistoryService
    {
        Task LogLoginAttemptAsync(int userId,  bool isSuccessful, string failureReason);
        Task LogLogoutAsync(int userId);
        Task<IEnumerable<LoginHistory>> GetUserLoginHistoryAsync(int userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<LoginHistory>> GetFailedLoginAttemptsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetFailedLoginAttemptsCountAsync(int userId, DateTime since);
    }

    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginHistoryService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor= httpContextAccessor;
        }

        public async Task LogLoginAttemptAsync(int userId, bool isSuccessful, string failureReason)
        {

            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            // Get IP Address (Handle Proxies)
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            var forwardedIp = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            // Use Forwarded IP if Available
            var clientIp = !string.IsNullOrEmpty(forwardedIp) ? forwardedIp : ipAddress;

            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);

            LoginHistory loginHistory = new LoginHistory
            {
                UserId = userId,
                LogInTime = DateTime.UtcNow,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                Device = string.Format("Browser: {0} {1}.{2}; OS: {3}", c.UA.Family, c.UA.Major, c.UA.Minor, c.OS.Family),
                IpAddress = clientIp
            };

            _db.LoginHistories.Add(loginHistory);
            await _db.SaveChangesAsync();
        }

        public async Task LogLogoutAsync(int userId)
        {

            var lastLogin = await _db.LoginHistories
                .Where(x => x.UserId == userId && x.IsSuccessful == true)
                .OrderByDescending(x => x.LogInTime)
                .FirstOrDefaultAsync();

            if (lastLogin != null)
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetUserLoginHistoryAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _db.LoginHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LogInTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginHistory>> GetFailedLoginAttemptsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.LoginHistories.Where(x => (bool)!x.IsSuccessful);

            if (startDate.HasValue)
                query = query.Where(x => x.LogInTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.LogInTime <= endDate.Value);

            return await query
                .OrderByDescending(x => x.LogInTime)
                .ToListAsync();
        }

        public async Task<int> GetFailedLoginAttemptsCountAsync(int userId, DateTime since)
        {
            return await _db.LoginHistories
                .CountAsync(x => x.UserId == userId &&
                                x.IsSuccessful != true &&
                                x.LogInTime >= since);
        }

        //private async Task<string> GetLocationFromIpAsync(string ipAddress)
        //{
        //    // You can implement IP geolocation here using a service like MaxMind GeoIP2
        //    // For now, returning null as it requires additional setup
        //    return null;
        //}
    }
}
