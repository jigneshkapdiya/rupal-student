using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RupalStudentCore8App.Server.Class.Configuration;
using Microsoft.Extensions.Options;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Init
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly IAuthService _authService;
        private readonly AppSettings _appSettings;

        public AuthController(
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db,
            IOptionsSnapshot<AppSettings> appSettings,
            IEmailService emailService,
            ISMSService smsService,
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            RoleManager<AspNetRole> roleManager,
            IAuthService authService,
            ILogger<AuthController> logger
            )
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _emailService = emailService;
            _logger = logger;
            _smsService = smsService;
            _authService = authService;
        }

        #endregion 

        #region Login 
        /// <summary>
        /// Authenticates a user using their username and password
        /// </summary>
        /// <param name="loginViewModel">Contains username and password</param>
        /// <returns>Auth result with tokens if successful, or 2FA challenge if required</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginViewModel)
        {
            try
            {
                // Attempt login
                var result = await _authService.LoginAsync(loginViewModel.Username, loginViewModel.Password);

                if (result.RequiresTwoFactor)
                    return Ok(result);

                if (!result.Succeeded)
                    return BadRequest(result.Message);

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", loginViewModel.Username);
                return BadRequest("An error occurred during login. Please try again later.");
            }
        }

        /// <summary>
        /// Completes the two-factor authentication process
        /// </summary>
        /// <param name="request">Contains userId, 2FA token, and verification code</param>
        /// <returns>Auth result with tokens if successful</returns>
        [HttpPost("login-2fa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginWith2fa([FromBody] LoginWith2faRequest request)
        {
            var result = await _authService.LoginWith2faAsync(request.UserId, request.TwoFactorToken, request.TwoFactorCode);

            if (!result.Succeeded)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// Validates Google ID token and authenticates the user
        /// </summary>
        /// <param name="request">Contains Google ID token and device information</param>
        /// <returns>Authentication result with user details and tokens</returns>
        [HttpPost("google-signin")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            // Call auth service for Google sign-in
            var result = await _authService.GoogleSignInAsync(request.IdToken);

            if (!result.Succeeded)
                return BadRequest(result.Message);

            return Ok(result);

        }

        [HttpPost("send-2fa-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send2faOTP([FromBody] Send2faOtpRequest request)
        {
            var result = await _authService.Send2faOtpAsync(request.Username);

            if (!result.Succeeded)
                return BadRequest(result.Message);

            return Ok(result);

        }


        /// <summary>
        /// Re-sends the two-factor authentication code
        /// </summary>
        /// <param name="request">Contains userId and 2FA session token</param>
        /// <returns>Success message if OTP is sent, or error details if failed</returns>
        [HttpPost("resend-2fa-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Resend2faOTP([FromBody] Resend2faOtpRequest request)
        {
            var result = await _authService.Resend2faOtpAsync(request.UserId, request.TwoFactorToken);

            if (!result.Succeeded)
                return BadRequest(result.Message);

            return Ok(result);

        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        /// <param name="request">Contains access token and refresh token</param>
        /// <returns>New access and refresh tokens if successful</returns>
        [HttpPost("token/refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

                if (!result.Succeeded)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return BadRequest("An error occurred while refreshing the token.");
            }
        }

        #endregion

        #region User Devices
        /// <summary>
        /// Gets a list of devices associated with the current user's account
        /// </summary>
        /// <returns>List of user devices with their details</returns>
        [HttpGet("devices")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserDevices()
        {
            try
            {
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                    return BadRequest("User data not found");

                var devices = await _db.UserDevices
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.LastLogin)
                    .Select(d => new
                    {
                        d.Id,
                        d.DeviceName,
                        d.DeviceType,
                        d.Os,
                        d.Browser,
                        d.IpAddress,
                        d.FirstLogin,
                        d.LastLogin,
                        d.IsActive,
                        d.IsRevoked
                    })
                    .ToListAsync();

                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user devices");
                return BadRequest("An error occurred while retrieving devices");
            }
        }

        /// <summary>
        /// Revokes access for a specific device, invalidating all its refresh tokens
        /// </summary>
        /// <param name="deviceId">The ID of the device to revoke</param>
        /// <returns>Success message and logout flag if current device is revoked</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("devices/{deviceId}/revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeDevice(int deviceId)
        {
            try
            {
                // Get and validate user ID
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                {
                    _logger.LogWarning($"Failed to revoke device {deviceId}: User ID not found in claims");
                    return BadRequest("User data not found");
                }

                // Find the device
                var device = await _db.UserDevices.FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);
                if (device == null)
                {
                    _logger.LogWarning($"Device {deviceId} not found for user {userId}");
                    return NotFound("Device not found or does not belong to user");
                }

                // Check if device is already revoked
                if (device.IsRevoked)
                {
                    _logger.LogInformation($"Device {deviceId} is already revoked");
                    return BadRequest("Device is already revoked");
                }

                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Revoke the device
                    device.IsActive = false;
                    device.IsRevoked = true;
                    device.RevokedDate = DateTime.UtcNow;

                    // Revoke all refresh tokens for this device
                    var refreshTokens = await _db.AspNetRefreshTokens
                        .Where(t => t.UserId == userId && t.DeviceIdentifier == device.DeviceIdentifier && t.RevokedOn == null)
                        .ToListAsync();

                    foreach (var token in refreshTokens)
                    {
                        token.RevokedOn = DateTime.UtcNow;
                        token.RevokedReason = "Device revoked";
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                _logger.LogInformation($"Device {deviceId} successfully revoked for user {userId}");

                // Check if revoking current device
                var currentDeviceId = Request.Headers["Device-Id"].FirstOrDefault();
                if (device.DeviceIdentifier == currentDeviceId)
                {
                    _logger.LogInformation("Current device {DeviceId} revoked, user will be logged out", deviceId);
                    return Ok(new { message = "Device revoked successfully", logout = true });
                }

                return Ok("Device revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking device {DeviceId}", deviceId);
                return BadRequest("An error occurred while revoking the device");
            }
        }

        #endregion

        #region Password Reset
        /// <summary>
        /// Initiates the password reset process by sending a reset link to the user's email
        /// </summary>
        /// <param name="request">Contains the user's email and client-side reset URL</param>
        /// <returns>Success message if email is sent, or error details if the process fails</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.ClientResetUrl))
                request.ClientResetUrl = _appSettings.Authentication.WebAppUrl + "/account/resetpassword";

            var result = await _authService.SendPasswordResetTokenAsync(request.Email, request.ClientResetUrl);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Resets a user's password using the token received via email
        /// </summary>
        /// <param name="resetPasswordViewModel">Contains email, reset token, and new password</param>
        /// <returns>Success message if password is reset, or error details if the process fails</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            if (user == null)
                return BadRequest("Invalid or expired password reset request.");

            // Replace any spaces with + in the token (common issue with URL-encoded tokens)
            resetPasswordViewModel.Token = resetPasswordViewModel.Token.Replace(" ", "+");

            var result = await _userManager.ResetPasswordAsync(
                user,
                resetPasswordViewModel.Token,
                resetPasswordViewModel.NewPassword);

            if (result.Succeeded)
                return Ok("Your password has been successfully reset. You can now log in with your new credentials.");

            return BadRequest(result.Errors.Select(e => e.Description));
        }
        #endregion

        #region Logout and Session Management

        /// <summary>
        /// Logs out the user from the current device and invalidates their tokens
        /// </summary>
        /// <returns>Success message if logout is successful</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                {
                    _logger.LogWarning("Failed to Logout");
                    return BadRequest("User data not found");
                }
                await _authService.LogoutAsync(userId);
                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest("An error occurred during logout");
            }
        }

        /// <summary>
        /// Gets all active sessions for the current user
        /// </summary>
        /// <returns>List of active sessions with device and location information</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("active-sessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                {
                    _logger.LogWarning("Failed to Logout");
                    return BadRequest("User data not found");
                }
                var sessions = await _authService.GetActiveSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
                return BadRequest("An error occurred while retrieving active sessions");
            }
        }

        /// <summary>
        /// Terminates all sessions except the current one
        /// </summary>
        /// <returns>Success message if all other sessions are terminated</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("terminate-other-sessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TerminateOtherSessions()
        {
            try
            {
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                {
                    _logger.LogWarning("Failed to Logout");
                    return BadRequest("User data not found");
                }

                await _authService.TerminateOtherSessionsAsync(userId);
                return Ok("All other sessions terminated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating other sessions");
                return BadRequest("An error occurred while terminating other sessions");
            }
        }

        #endregion
    }
}




