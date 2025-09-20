using Microsoft.AspNetCore.Identity;

namespace RupalStudentCore8App.Server.Models.Auth
{
    /// <summary>
    /// Represents the result of an authentication operation with JWT token support
    /// </summary>
    public class AuthResult : SignInResult
    {
        /// <summary>
        /// Gets or sets the user ID when two-factor authentication is required
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the JWT access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the two-factor authentication token
        /// </summary>
        public string TwoFactorToken { get; set; }

        /// <summary>
        /// Gets or sets a success message for successful operations
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a successful authentication result with tokens
        /// </summary>
        public static new AuthResult Success(string accessToken, string refreshToken, string userId="0")
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            return new AuthResult
            {
                UserId = userId,
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Creates a result indicating two-factor authentication is required
        /// </summary>
        public static new AuthResult TwoFactorRequired(string userId)
        {
            //if (userId <= 0)
            //    throw new ArgumentException("User ID must be positive", nameof(userId));

            return new AuthResult
            {
                Succeeded = false,
                RequiresTwoFactor = true,
                UserId = userId
            };
        }

        /// <summary>
        /// Creates a result indicating two-factor authentication is required
        /// </summary>
        public static new AuthResult TwoFactorRequired(string userId, string twoFactorToken)
        {
            return new AuthResult
            {
                Succeeded = true,
                RequiresTwoFactor = true,
                UserId = userId,
                TwoFactorToken = twoFactorToken
            };
        }

        /// <summary>
        /// Creates a successful result with a message
        /// </summary>
        public static AuthResult SuccessWithMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            return new AuthResult
            {
                Succeeded = true,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failed authentication result with an error message
        /// </summary>
        public static new AuthResult Failed(string error)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));

            return new AuthResult
            {
                Succeeded = false,
                Message = error
            };
        }

        

    }


}
