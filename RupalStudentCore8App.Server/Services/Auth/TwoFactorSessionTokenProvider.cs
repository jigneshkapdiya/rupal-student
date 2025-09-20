using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace RupalStudentCore8App.Server.Services.Auth
{
    // Provider for two-factor authentication session tokens
    public class TwoFactorSessionTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : class
    {
        private const int SESSION_LIFETIME_MINUTES = 10;
        private const int TOKEN_BYTES = 32;

        public async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            // Generate a random token
            var bytes = new byte[TOKEN_BYTES];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Include expiration timestamp and purpose in the token
            var expiry = DateTime.UtcNow.AddMinutes(SESSION_LIFETIME_MINUTES);
            var tokenData = $"{Convert.ToBase64String(bytes)}.{expiry.Ticks}.{purpose}";

            // Add entropy based on user data to prevent token reuse
            var userStamp = await manager.GetSecurityStampAsync(user);
            var finalToken = $"{tokenData}.{userStamp}";

            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(finalToken));
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            try
            {
                // Decode the token
                var tokenBytes = Convert.FromBase64String(token);
                var tokenString = System.Text.Encoding.UTF8.GetString(tokenBytes);
                var parts = tokenString.Split('.');

                if (parts.Length != 4)
                    return false;

                var tokenValue = parts[0];
                var expiryTicks = long.Parse(parts[1]);
                var tokenPurpose = parts[2];
                var userStamp = parts[3];

                // Validate expiration
                var expiry = new DateTime(expiryTicks);
                if (DateTime.UtcNow > expiry)
                    return false;

                // Validate purpose
                if (tokenPurpose != purpose)
                    return false;

                // Validate user security stamp to ensure user hasn't changed
                var currentStamp = await manager.GetSecurityStampAsync(user);
                if (currentStamp != userStamp)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }
    }
}
