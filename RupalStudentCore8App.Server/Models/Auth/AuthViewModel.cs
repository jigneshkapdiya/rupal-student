using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.Models.Auth
{
    public class AuthViewModel
    {
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 100 characters")]
        public required string Password { get; set; }
    }
    public class LoginWith2faRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the two-factor authentication session token.
        /// </summary>
        [Required(ErrorMessage = "Two Factor Session Token is required.")]
        public required string TwoFactorToken { get; set; }

        /// <summary>
        /// Gets or sets the two-factor authentication code (OTP).
        /// </summary>
        [Required(ErrorMessage = "Two Factor Code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Two Factor Code must be exactly 6 characters.")]
        public required string TwoFactorCode { get; set; }

    }

    public class GoogleSignInRequest
    {
        [Required(ErrorMessage = "Google id token is required.")]
        public required string IdToken { get; set; }
    }

    public class RefreshTokenRequest
    {
        //[Required]
        //public string RefreshToken { get; set; }
        //[Required]
        //public int UserId { get; set; }

        [Required]
        public required string AccessToken { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(8, ErrorMessage = "New Password must be minimum 8 character.")]
        [MaxLength(20, ErrorMessage = "New Password must not be more then 20 character.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Invalid reset password token.")]
        public required string Token { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old Password is required.")]
        public required string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be 8 to 20 character long")]
        [MaxLength(20, ErrorMessage = "Password must be 8 to 20 character long")]
        public required string NewPassword { get; set; }
    }

    public class Send2faOtpRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }
    }

    public class Resend2faOtpRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "Two Factor Session Token is required.")]
        public required string TwoFactorToken { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// Client URL where the user will be redirected to reset their password
        /// </summary>
       
        public string? ClientResetUrl { get; set; }
    }

    public class GoogleTokenPayload
    {
        public string Sub { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
    }

}
