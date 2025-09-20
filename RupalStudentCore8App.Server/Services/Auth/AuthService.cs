using RupalStudentCore8App.Server.Class;
using RupalStudentCore8App.Server.Class.Auth;
using RupalStudentCore8App.Server.Class.Configuration;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.Services;
using RupalStudentCore8App.Server.Services.Auth;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> LoginWith2faAsync(string userId, string twoFactorToken, string twoFactorCode);
    Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<AuthResult> Send2faOtpAsync(string username);
    Task<AuthResult> Resend2faOtpAsync(string userId, string twoFactorToken);
    Task<AuthResult> SendPasswordResetTokenAsync(string email, string clientResetUrl);
    Task<AuthResult> GoogleSignInAsync(string idToken);
    Task LogoutAsync(int userId);
    Task<List<SessionInfo>> GetActiveSessionsAsync(int userId);
    Task TerminateOtherSessionsAsync(int userId);
}


public class AuthService : IAuthService
{
    private readonly UserManager<AspNetUser> _userManager;
    private readonly RoleManager<AspNetRole> _roleManager;
    private readonly SignInManager<AspNetUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationSettings _authConfig;
    private readonly GoogleSettings _googleConfig;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string OTP_COOL_DOWN_PREFIX = "otp:cool-down";
    private const string OTP_ATTEMPTS_PREFIX = "otp:attempts";

    public AuthService(
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager,
        SignInManager<AspNetUser> signInManager,
        ITokenService tokenService,
        IEmailService emailService,
        ISMSService smsService,
        ApplicationDbContext context,
        IOptions<AuthenticationSettings> authConfig,
        IOptions<GoogleSettings> googleConfig,
        IDeviceInfoService deviceInfoService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger,
        IDistributedCache cache,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _smsService = smsService;
        _context = context;
        _authConfig = authConfig.Value;
        _googleConfig = googleConfig.Value;
        _deviceInfoService = deviceInfoService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Authenticates a user using their username/email and password
    /// </summary>
    /// <param name="username">Username or email of the user</param>
    /// <param name="password">User's password</param>
    /// <returns>
    /// AuthResult containing:
    /// - Success status
    /// - Access/refresh tokens for non-2FA users
    /// - 2FA challenge for users with 2FA enabled
    /// - Error message if authentication fails
    /// </returns>
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            // Get device info from request headers
            var deviceInfo = _deviceInfoService.GetDeviceInfo();

            // Find user by username or email
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            if (user == null)
            {
                return AuthResult.Failed("Invalid login credentials.");
            }

            if (!user.Status)
            {
                return AuthResult.Failed("Your account has been blocked. Please contact administrator.");
            }

            // Check for account lockout
            if (await _userManager.IsLockedOutAsync(user))
            {
                return AuthResult.Failed("Account temporarily locked. Please try again later.");
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return AuthResult.Failed("Account temporarily locked due to multiple failed attempts.");
                }
                return AuthResult.Failed("Invalid login credentials.");
            }

            // Reset failed access attempts on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            // Validate user roles
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                return AuthResult.Failed("Access denied. You do not have the required permissions to access this panel.");
            }

            // Check if user has 2FA enabled
            if (user.TwoFactorEnabled)
            {
                // Validate phone number or email for 2FA
                if (string.IsNullOrEmpty(user.PhoneNumber) && string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogError($"2FA configuration error: No phone/email for user {username}");
                    return AuthResult.Failed("Two-factor authentication is enabled, but no phone number or email is associated with the account.");
                }

                // Initialize 2FA
                /// <summary>
                /// Generates and sends two-factor authentication tokens to a user
                /// </summary>
                /// <param name="user">The user requiring 2FA</param>
                /// <param name="deviceIdentifier">Optional device identifier for device-specific tokens</param>
                /// <returns>
                /// AuthResult containing:
                /// - Session token for 2FA verification
                /// - Error message if token generation/sending fails
                /// </returns>
                /// <remarks>
                /// This method:
                /// 1. Generates a session token for 2FA verification
                /// 2. Stores session token with device association
                /// 3. Generates one-time password (OTP)
                /// 4. Sends OTP via SMS and/or email
                /// 5. Returns session token for subsequent verification
                /// </remarks>
                return await GenerateAndSendTwoFactorTokensAsync(user, deviceInfo?.DeviceIdentifier);
            }
            else
            {
                // For non-2FA users, issue tokens directly
                return await GenerateTokens(user, deviceInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during login for user {username}");
            return AuthResult.Failed("An unexpected error occurred during login.");
        }
    }

    /// <summary>
    /// Completes the two-factor authentication login process by validating the OTP
    /// </summary>
    /// <param name="userId">ID of the user attempting to log in</param>
    /// <param name="twoFactorToken">Session token from initial 2FA request</param>
    /// <param name="twoFactorCode">OTP code entered by the user</param>
    /// <returns>
    /// AuthResult containing:
    /// - Access and refresh tokens if verification succeeds
    /// - Error message if verification fails
    /// </returns>
    /// <remarks>
    /// This method implements a secure 2FA verification flow:
    /// 1. Validates user exists and has 2FA enabled
    /// 2. Verifies session token authenticity and expiration
    /// 3. Validates OTP code
    /// 4. Resets OTP cooldown on success
    /// 5. Removes used session token
    /// 6. Issues new access/refresh tokens
    /// 
    /// Security measures:
    /// - Device-specific session tokens
    /// - One-time use session tokens
    /// - OTP cooldown reset after successful login
    /// - Comprehensive error logging
    /// </remarks>
    public async Task<AuthResult> LoginWith2faAsync(string userId, string twoFactorToken, string twoFactorCode)
    {
        try
        {
            // Step 1: Validate user exists and is active
            // Ensure the user exists and is active before proceeding with 2FA
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"2FA login attempted for non-existent user ID: {userId}");
                return AuthResult.Failed("User not found");
            }

            // Step 2: Validate 2FA status
            // Ensure 2FA is enabled for the user before proceeding
            if (!user.TwoFactorEnabled)
            {
                return AuthResult.Failed("Two-factor authentication is not enabled for this user");
            }

            // Step 2: Get device information for token management
            var deviceInfo = _deviceInfoService.GetDeviceInfo();
            string tokenName = (deviceInfo?.DeviceIdentifier != null) ?
                $"2FAToken_{deviceInfo.DeviceIdentifier}" : "2FAToken";

            // Step 3: Validate session token
            // Ensure the session token exists and matches what was provided
            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider.TwoFactor, tokenName);
            if (string.IsNullOrEmpty(storedToken))
            {
                _logger.LogWarning($"2FA session token not found for user {userId} and device {deviceInfo?.DeviceIdentifier}");
                return AuthResult.Failed("2FA session token not found or already used");
            }

            // Step 4: Validate the token's authenticity and match
            // This ensures the token is valid and matches what was issued during login
            var tokenValidation = await _userManager.VerifyTwoFactorTokenAsync(user, LoginProvider.TwoFactor, twoFactorToken);

            if (!tokenValidation || storedToken != twoFactorToken)
            {
                _logger.LogWarning($"Invalid 2FA session token for user {userId}");
                return AuthResult.Failed("Invalid or expired 2FA session token");
            }

            // Step 4: Verify OTP code
            // Validate the user-provided OTP against the stored code
            var validVerification = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, twoFactorCode);
            if (!validVerification)
            {
                _logger.LogWarning($"Invalid 2FA code provided for user {userId}");
                return AuthResult.Failed("Invalid verification code");
            }

            // Step 5: Cleanup after successful verification
            // Remove the used session token to prevent reuse
            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider.TwoFactor, tokenName);

            // Reset OTP cooldown since login was successful
            // This allows immediate OTP requests if needed for another device
            var key = $"{OTP_COOL_DOWN_PREFIX}:{userId}:{deviceInfo?.DeviceIdentifier}";
            await _cache.RemoveAsync(key);

            // Step 6: Generate authentication tokens
            // Create new access and refresh tokens for the authenticated session
            _logger.LogInformation($"Generating authentication tokens for verified user {userId}");
            return await GenerateTokens(user, deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during 2FA login for user ID: {userId}");
            return AuthResult.Failed("An unexpected error occurred during 2FA verification.");
        }
    }



    public async Task<AuthResult> Send2faOtpAsync(string username)
    {
        try
        {
            // Step 1:
            // Find user by username or email
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            if (user == null)
            {
                // here even if user is not found, we return success to prevent email enumeration attacks
                return AuthResult.SuccessWithMessage("we sent you a verification code, please check your email or phone.");
            }

            if (!user.Status)
            {
                return AuthResult.Failed("Your account has been blocked. Please contact administrator.");
            }

            // Check for account lockout
            if (await _userManager.IsLockedOutAsync(user))
            {
                return AuthResult.Failed("Account temporarily locked. Please try again later.");
            }

            // Step 2: Get device information for token management
            // Extract device info to create device-specific token names
            var deviceInfo = _deviceInfoService.GetDeviceInfo();
            return await GenerateAndSendTwoFactorTokensAsync(user, deviceInfo.DeviceIdentifier);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during send otp verification code for user : {username}");
            return AuthResult.Failed("An unexpected error occurred during send OTP verification code.");
        }
    }

    /// <summary>
    /// Re-sends a two-factor authentication OTP to a user after validating cool-down and daily limits
    /// </summary>
    /// <param name="userId">The ID of the user requesting OTP resend</param>
    /// <param name="twoFactorToken">The session token from the initial 2FA request</param>
    /// <returns>
    /// AuthResult containing:
    /// - Success message if OTP is sent
    /// - Error message if resend fails or is rate-limited
    /// </returns>
    /// <remarks>
    /// This method implements rate limiting through:
    /// 1. Per-device cool-down period between OTP requests
    /// 2. Daily maximum OTP attempts per user
    /// 3. Session token validation to prevent unauthorized re-sends
    /// </remarks>
    public async Task<AuthResult> Resend2faOtpAsync(string userId, string twoFactorToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"OTP resend attempted for non-existent user ID: {userId}");
                return AuthResult.Failed("User not found");
            }

            // Step 2: Get device information for token management
            // Extract device info to create device-specific token names
            var deviceInfo = _deviceInfoService.GetDeviceInfo();
            string tokenName = (deviceInfo?.DeviceIdentifier != null) ?
                $"2FAToken_{deviceInfo.DeviceIdentifier}" : "2FAToken";

            // Step 3: Check device-specific cool-down period
            // Prevent rapid OTP requests from same device
            var (isInCooldown, remainingSeconds) = await CheckOtpCooldown(userId, deviceInfo?.DeviceIdentifier);
            if (isInCooldown)
                return AuthResult.Failed($"Please wait {remainingSeconds} seconds before requesting a new OTP.");

            // Step 4: Enforce daily OTP attempt limit
            // Prevent excessive OTP requests within 24 hours
            if (!await CheckAndIncrementDailyAttempts(userId))
                return AuthResult.Failed("Daily OTP limit reached. Please try again tomorrow.");

            // Step 5: Validate session token exists
            // Verify this is a legitimate 2FA session and token hasn't been used
            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider.TwoFactor, tokenName);
            if (string.IsNullOrEmpty(storedToken))
                return AuthResult.Failed("2FA session token not found or already used");

            // Step 6: Verify token authenticity
            // Ensure token is valid and matches what was issued during login
            var tokenValidation = await _userManager.VerifyTwoFactorTokenAsync(user, LoginProvider.TwoFactor, twoFactorToken);

            if (!tokenValidation || storedToken != twoFactorToken)
            {
                return AuthResult.Failed("Invalid or expired 2FA session token");
            }

            // Step 7: Generate and send new OTP
            // Use dedicated method to handle OTP generation and delivery
            var result = await ReSendTwoFactorCodeAsync(user);
            if (!result.Succeeded)
                return result;

            // Step 8: Update cool-down timestamp
            // Record this attempt to enforce future cool-down
            await SetOtpCooldown(userId, deviceInfo?.DeviceIdentifier);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during re-send 2fa code for user ID: {userId}");
            return AuthResult.Failed("An unexpected error occurred during resend 2FA verification code.");
        }
    }



    #region OTP Cool Down & Increment Daily Attempts
    /// <summary>
    /// Checks if a user's OTP request is within the cool-down period for a specific device.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the OTP.</param>
    /// <param name="deviceId">The ID of the device making the request.</param>
    /// <returns>A tuple containing whether the request is in cool-down period and remaining seconds if applicable.</returns>
    /// <remarks>The cool-down period helps prevent abuse by limiting how frequently users can request new OTPs.</remarks>
    private async Task<(bool isInCooldown, int remainingSeconds)> CheckOtpCooldown(string userId, string deviceId)
    {
        try
        {
            var key = $"{OTP_COOL_DOWN_PREFIX}:{userId}:{deviceId}";
            var lastSendTime = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(lastSendTime))
                return (false, 0);

            if (!DateTime.TryParse(lastSendTime, out DateTime lastTime))
            {
                _logger.LogWarning("Invalid timestamp format in OTP cool-down cache for user {UserId}", userId);
                return (false, 0);
            }

            var timeSinceLastSend = DateTime.UtcNow - lastTime;
            if (timeSinceLastSend.TotalSeconds < _authConfig.OtpResendCooldownSeconds)
            {
                var remaining = (int)(_authConfig.OtpResendCooldownSeconds - timeSinceLastSend.TotalSeconds);
                return (true, remaining);
            }

            return (false, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP cool-down period for user {UserId}", userId);
            return (false, 0); // Fail open to allow OTP in case of cache errors
        }
    }

    /// <summary>
    /// Checks and increments the daily OTP attempt counter for a user.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the OTP.</param>
    /// <returns>True if the user hasn't exceeded daily limits, false otherwise.</returns>
    private async Task<bool> CheckAndIncrementDailyAttempts(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var key = $"{OTP_ATTEMPTS_PREFIX}:{userId}:{today}";
            var attempts = await _cache.GetStringAsync(key);
            int currentAttempts = 0;

            if (!string.IsNullOrEmpty(attempts) && !int.TryParse(attempts, out currentAttempts))
            {
                _logger.LogWarning("Invalid attempt count in cache for user {UserId}", userId);
                currentAttempts = 0;
            }

            if (currentAttempts >= _authConfig.MaxDailyOtpAttempts)
            {
                _logger.LogWarning(
                    "User {UserId} exceeded daily OTP limit of {MaxAttempts}",
                    userId,
                    _authConfig.MaxDailyOtpAttempts);
                return false;
            }

            // Set expiration to end of current day UTC
            var expiryTime = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiryTime
            };

            await _cache.SetStringAsync(key, (currentAttempts + 1).ToString(), options);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing daily OTP attempts for user {UserId}", userId);
            return false; // Fail closed for security
        }
    }

    /// <summary>
    /// Sets the cool-down period timestamp for a user's OTP request on a specific device.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the OTP.</param>
    /// <param name="deviceId">The ID of the device making the request.</param>
    /// <remarks>The cool-down period prevents users from requesting OTPs too frequently.</remarks>
    private async Task SetOtpCooldown(string userId, string deviceId)
    {
        try
        {
            var key = $"{OTP_COOL_DOWN_PREFIX}:{userId}:{deviceId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_authConfig.OtpResendCooldownSeconds)
            };

            var timestamp = DateTime.UtcNow.ToString("O"); // ISO 8601 format
            await _cache.SetStringAsync(key, timestamp, options);

        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set OTP cool-down period for user {UserId} on device {DeviceId}",
                userId,
                deviceId);
            // Don't rethrow as this is non-critical
        }
    }

    #endregion

    /// <summary>
    /// Sends a password reset token to the user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="clientResetUrl">URL for password reset page</param>
    /// <returns>Auth result with success message or error</returns>
    public async Task<AuthResult> SendPasswordResetTokenAsync(string email, string clientResetUrl)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Return success even if user not found to prevent email enumeration
                return AuthResult.SuccessWithMessage("If your email is registered, you will receive password reset instructions.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create reset URL with token
            var resetUrl = $"{clientResetUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            await _emailService.SendPasswordResetLinkAsync(user.Email, user.FullName, resetUrl);

            return AuthResult.SuccessWithMessage("Password reset instructions have been sent to your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for email: {Email}", email);
            return AuthResult.Failed("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token
    /// </summary>
    /// <param name="accessToken">Expired access token</param>
    /// <param name="refreshToken">Valid refresh token</param>
    /// <param name="deviceInfo">Device information for token management</param>
    /// <returns>Auth result with new access and refresh tokens if successful</returns>
    public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            // Extract user information from the expired access token
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                _logger.LogWarning("Invalid access token during refresh attempt");
                return AuthResult.Failed("Invalid access token");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Missing user ID claim in access token");
                return AuthResult.Failed("Invalid token claims");
            }

            // Find user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User not found during token refresh. User ID: {userId}");
                return AuthResult.Failed("User not found");
            }

            // Start transaction for token rotation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var deviceInfo = _deviceInfoService.GetDeviceInfo();

                // Validate existing refresh token
                var existingToken = await _context.AspNetRefreshTokens
                    .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId) &&
                                             t.Token == refreshToken &&
                                             t.DeviceIdentifier == deviceInfo.DeviceIdentifier);

                if (existingToken == null)
                {
                    _logger.LogWarning($"Invalid refresh token attempt for user {userId} from device {deviceInfo.DeviceIdentifier}");
                    return AuthResult.Failed("Invalid refresh token");
                }

                if (existingToken.ExpireOn < DateTime.UtcNow)
                {
                    _logger.LogInformation($"Expired refresh token used for user {userId}");
                    return AuthResult.Failed("Refresh token has expired");
                }

                if (existingToken.IsRevoked)
                {
                    _logger.LogWarning($"Attempt to use revoked refresh token for user {userId}");
                    return AuthResult.Failed("Refresh token has been revoked");
                }

                // Generate new tokens
                var newAccessToken = await _tokenService.GenerateAccessToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Update refresh token in database
                //existingToken.IsRevoked = true;
                existingToken.RevokedOn = DateTime.UtcNow;
                existingToken.RevokedReason = "Refresh token rotation";

                var newRefreshTokenEntity = new AspNetRefreshToken
                {
                    Token = newRefreshToken,
                    UserId = int.Parse(userId),
                    CreatedOn = DateTime.UtcNow,
                    ExpireOn = DateTime.UtcNow.AddMinutes(_authConfig.RefreshTokenExpirationMinutes),
                    DeviceIdentifier = deviceInfo.DeviceIdentifier
                };

                _context.AspNetRefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                //_logger.LogInformation($"Successfully refreshed tokens for user {userId}");
                return AuthResult.Success(newAccessToken, newRefreshToken, userId);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Let the outer catch block handle it
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            return AuthResult.Failed("Failed to refresh token");
        }
    }

    /// <summary>
    /// Generates new access and refresh tokens for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="deviceInfo">Device information for token management</param>
    /// <returns>Auth result with new access and refresh tokens if successful</returns>
    private async Task<AuthResult> GenerateTokens(AspNetUser user, DeviceInfo deviceInfo)
    {
        try
        {

            // Manage device login
            var device = await ManageUserDevice(user, deviceInfo);
            if (device == null)
            {
                _logger.LogWarning($"Token generation failed: Device limit exceeded for user {user.UserName}");
                return AuthResult.Failed("Device limit exceeded");
            }

            //_logger.LogInformation($"Device validated for user {user.UserName}: {device.DeviceIdentifier}");

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            //_logger.LogInformation($"Generated new access and refresh tokens for user {user.UserName}");

            // Save refresh token
            var newRefreshToken = new AspNetRefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedOn = DateTime.UtcNow,
                ExpireOn = DateTime.UtcNow.AddMinutes(_authConfig.RefreshTokenExpirationMinutes),
                DeviceIdentifier = device.DeviceIdentifier
            };

            _context.AspNetRefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
            //_logger.LogInformation($"Saved new refresh token for user {user.UserName} with expiry {newRefreshToken.ExpireOn}");

            return AuthResult.Success(accessToken, refreshToken, user.Id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate tokens for user {user.UserName}");
            throw; // Re-throw to let caller handle the error
        }
    }

    /// <summary>
    /// Generates and sends two-factor authentication tokens to the user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="deviceIdentifier">Device identifier for token management</param>
    /// <returns>Auth result with success message or error</returns>
    private async Task<AuthResult> GenerateAndSendTwoFactorTokensAsync(AspNetUser user, string deviceIdentifier = "")
    {
        try
        {

            // 1. Generate session token with built-in expiration and validation
            var twoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, LoginProvider.TwoFactor);

            // 2. Store session token for later validation
            string tokenName = !String.IsNullOrEmpty(deviceIdentifier) ? $"2FAToken_{deviceIdentifier}" : "2FAToken";
            await _userManager.SetAuthenticationTokenAsync(user, LoginProvider.TwoFactor, tokenName, twoFactorToken);

            // 3. Generate OTP
            var otpToken = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

            // 4. Send OTP via SMS if phone number is available
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                await _smsService.SendSmsAsync($"Your verification code is: {otpToken}", user.PhoneNumber);
                _logger.LogInformation($"Sent OTP {otpToken} via SMS to user {user.UserName}");
            }
            else
            {
                _logger.LogInformation($"No phone number available for user {user.UserName}, skipping SMS");
            }

            // 5. Send OTP via email if email is available
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailBody = $"Your one-time password is: {otpToken}. This code will expire in 10 minutes.";
                await _emailService.SendEmailAsync("Two-Factor Authentication Code", emailBody, user.Email, user.FullName);
                _logger.LogInformation($"Sent OTP via email to user {user.UserName}");
            }
            else
            {
                _logger.LogInformation($"No email available for user {user.UserName}, skipping email");
            }

            // 6. Return result with session token
            return AuthResult.TwoFactorRequired(user.Id.ToString(), twoFactorToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize 2FA for user {user.UserName}");
            return AuthResult.Failed("Failed to initialize two-factor authentication.");
        }
    }

    /// <summary>
    /// Generates and sends a new OTP to a user via SMS and/or email
    /// </summary>
    /// <param name="user">The user to send the OTP to</param>
    /// <returns>
    /// AuthResult containing:
    /// - Success message if OTP is sent successfully
    /// - Error message if sending fails
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Generates a new OTP using phone provider format
    /// 2. Attempts delivery via SMS if phone number exists
    /// 3. Attempts delivery via email as backup
    /// 4. Ensures at least one delivery method succeeds
    /// </remarks>
    private async Task<AuthResult> ReSendTwoFactorCodeAsync(AspNetUser user)
    {
        try
        {
            // Step 1: Generate new OTP using phone provider format
            // This ensures consistent OTP format regardless of delivery method
            var otpToken = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

            // Step 2: Attempt SMS delivery
            // Primary delivery method if phone number is verified
            bool smsSent = false;
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                await _smsService.SendSmsAsync($"Your verification code is: {otpToken}", user.PhoneNumber);
                _logger.LogInformation($"Sent OTP via SMS to user {user.UserName} at {user.PhoneNumber}");
                smsSent = true;
            }
            else
            {
                _logger.LogInformation($"Skipping SMS for user {user.UserName}: no phone number available");
            }

            // Step 3: Attempt email delivery
            // Backup delivery method and/or additional security layer
            bool emailSent = false;
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailBody = $"Your one-time password is: {otpToken}. This code will expire in 10 minutes.";
                await _emailService.SendEmailAsync("Two-Factor Authentication Code", emailBody, user.Email, user.FullName);
                _logger.LogInformation($"Sent OTP via email to user {user.UserName} at {user.Email}");
                emailSent = true;
            }
            else
            {
                _logger.LogInformation($"Skipping email for user {user.UserName}: no email available");
            }

            // Step 4: Verify at least one delivery method succeeded
            if (!smsSent && !emailSent)
            {
                _logger.LogError($"Failed to send OTP: no valid delivery method for user {user.UserName}");
                return AuthResult.Failed("Unable to send verification code. Please contact support.");
            }

            // Return success if we reach here (at least one delivery method worked)
            _logger.LogInformation($"Successfully sent OTP to user {user.UserName} via {(smsSent ? "SMS" : "")} {(emailSent ? "email" : "")}");
            return AuthResult.SuccessWithMessage("Verification code sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to resend 2FA code for user {user.UserName}");
            return AuthResult.Failed("Failed to resend two-factor authentication code.");
        }
    }


    /// <summary>
    /// Authenticates a user using a Google ID token and returns the result of the sign-in attempt.
    /// </summary>
    /// <remarks>This method validates the provided Google ID token, checks for an existing user account
    /// associated with the token's email, and verifies the user's status and roles. If authentication is successful,
    /// the user's email is auto-confirmed and authentication tokens are generated. The method handles common failure
    /// scenarios, such as expired tokens, blocked accounts, locked-out users, and insufficient permissions.</remarks>
    /// <param name="idToken">The Google ID token obtained from the client after a successful Google sign-in. This token must be valid and not
    /// expired.</param>
    /// <returns>An <see cref="AuthResult"/> representing the outcome of the authentication attempt. If successful, the result
    /// contains authentication tokens; otherwise, it includes an error message describing the reason for failure.</returns>
    public async Task<AuthResult> GoogleSignInAsync(string idToken)
    {
        try
        {
            if (string.IsNullOrEmpty(idToken))
                return AuthResult.Failed("Google ID token is required.");

            // Validate Google token using official library
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _googleConfig.ClientId }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                if (payload == null)
                    return AuthResult.Failed("Google sign-in attempt failed.");

                if (string.IsNullOrEmpty(payload.Email))
                    return AuthResult.Failed("Google sign-in attempt failed. Email not found in token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token validation failed");

                if (ex.Message.Contains("JWT has expired"))
                    return AuthResult.Failed("Your Google session expired. Please try again.");
                return AuthResult.Failed("Google sign-in attempt failed.");
            }

            // Find or validate existing user
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
                return AuthResult.Failed($"No account registered with email {payload.Email}");

            // Validate user status
            if (!user.Status)
            {
                return AuthResult.Failed("Your account has been blocked. Please contact administrator.");
            }

            // Check for account lockout
            if (await _userManager.IsLockedOutAsync(user))
            {
                return AuthResult.Failed("Account temporarily locked. Please try again later.");
            }

            // Validate user roles
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
                return AuthResult.Failed("Access denied. You do not have the required permissions to access this panel.");

            // Auto-confirm email for Google-authenticated users
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            // Attempt sign in
            var signInResult = await _signInManager.ExternalLoginSignInAsync("Google", payload.Subject, isPersistent: false, bypassTwoFactor: true);

            if (!signInResult.Succeeded)
            {
                // Add external login if it doesn't exist
                var info = new UserLoginInfo("Google", payload.Subject, "Google");
                await _userManager.AddLoginAsync(user, info);
            }

            // Get device info from request headers
            var deviceInfo = _deviceInfoService.GetDeviceInfo();
            // Generate auth result using existing login flow
            return await GenerateTokens(user, deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google sign-in");
            return AuthResult.Failed("An error occurred during login. Please try again later.");
        }
    }


    /// <summary>
    /// Validates a Google reCAPTCHA response token
    /// </summary>
    /// <param name="captchaResponse">The reCAPTCHA response token from the client</param>
    /// <returns>True if the captcha is valid, false otherwise</returns>
    public async Task<bool> ValidateGoogleCaptchaAsync(string captchaResponse)
    {
        try
        {
            if (string.IsNullOrEmpty(captchaResponse))
            {
                _logger.LogWarning("Empty captcha response received");
                return false;
            }

            string secretKey = _googleConfig.ReCaptcha.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Google reCAPTCHA secret key not configured");
                throw new InvalidOperationException("reCAPTCHA configuration missing");
            }

            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}");

            var verification = JObject.Parse(response);
            bool isValid = verification.Value<bool>("success");

            if (!isValid)
            {
                var errorCodes = verification["error-codes"]?.ToObject<string[]>();
                if (errorCodes?.Any() == true)
                {
                    _logger.LogWarning("reCAPTCHA validation failed with errors: {Errors}", string.Join(", ", errorCodes));
                }
            }

            return isValid;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to validate reCAPTCHA with Google API");
            throw new InvalidOperationException("Failed to validate reCAPTCHA", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse reCAPTCHA response");
            throw new InvalidOperationException("Invalid reCAPTCHA response format", ex);
        }
    }

    private async Task<UserDevice?> ManageUserDevice(AspNetUser user, DeviceInfo deviceInfo)
    {
        try
        {
            var existingDevice = await _context.UserDevices.FirstOrDefaultAsync(d => d.UserId == user.Id && d.DeviceIdentifier == deviceInfo.DeviceIdentifier);

            if (existingDevice != null)
            {
                // Update existing device with all required fields
                existingDevice.LastLogin = DateTime.UtcNow;
                existingDevice.IpAddress = deviceInfo.IpAddress;
                existingDevice.DeviceName = deviceInfo.DeviceName;
                existingDevice.DeviceType = deviceInfo.DeviceType;
                existingDevice.Os = deviceInfo.OS;
                existingDevice.Browser = deviceInfo.Browser;
                existingDevice.IsActive = true;
                existingDevice.IsRevoked = false;
                existingDevice.RevokedDate = null;

                _context.UserDevices.Update(existingDevice);

                // Revoke any other refresh tokens for this device
                var oldTokens = await _context.AspNetRefreshTokens
                    .Where(t => t.UserId == user.Id && t.DeviceIdentifier == deviceInfo.DeviceIdentifier && t.RevokedOn == null)
                    .ToListAsync();

                foreach (var token in oldTokens)
                {
                    token.RevokedOn = DateTime.UtcNow;
                    token.RevokedReason = "New login from same device";
                    _context.AspNetRefreshTokens.Update(token);
                }

                await _context.SaveChangesAsync();
                return existingDevice;
            }

            // Check device limit
            var activeDevices = await _context.UserDevices
                .Where(d => d.UserId == user.Id && d.IsActive && !d.IsRevoked)
                .OrderByDescending(d => d.LastLogin)
                .ToListAsync();

            if (activeDevices.Count >= _authConfig.AllowedDevices)
            {
                // Revoke oldest device and its tokens
                var oldestDevice = activeDevices.Last();
                oldestDevice.IsActive = false;
                oldestDevice.IsRevoked = true;
                oldestDevice.RevokedDate = DateTime.UtcNow;

                // Revoke old token
                var oldTokens = await _context.AspNetRefreshTokens
                    .Where(t => t.DeviceIdentifier == oldestDevice.DeviceIdentifier && !t.IsRevoked)
                    .ToListAsync();

                foreach (var token in oldTokens)
                {
                    token.RevokedOn = DateTime.UtcNow;
                    token.RevokedReason = "Device limit exceeded";
                }
            }

            // Create new device with all required fields
            var newDevice = new UserDevice
            {
                UserId = user.Id,
                DeviceIdentifier = deviceInfo.DeviceIdentifier,
                DeviceName = deviceInfo.DeviceName,
                DeviceType = deviceInfo.DeviceType,
                Os = deviceInfo.OS,
                Browser = deviceInfo.Browser,
                IpAddress = deviceInfo.IpAddress,
                FirstLogin = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true,
                IsRevoked = false
            };

            await _context.UserDevices.AddAsync(newDevice);
            await _context.SaveChangesAsync();
            return newDevice;

        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Logs out a user from a specific device by revoking their refresh tokens
    /// </summary>
    /// <param name="userId">ID of the user to log out</param>
    /// <param name="deviceId">ID of the device to log out from</param>
    public async Task LogoutAsync(int userId)
    {
        try
        {
            var deviceInfo = _deviceInfoService.GetDeviceInfo();

            // Get the device
            var device = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceIdentifier == deviceInfo.DeviceIdentifier);

            if (device == null)
            {
                _logger.LogWarning("Device {DeviceId} not found for user {UserId} during logout", deviceInfo.DeviceName, userId);
                return;
            }

            // Revoke all refresh tokens for this device
            var tokens = await _context.AspNetRefreshTokens
                .Where(t => t.UserId == userId && t.DeviceIdentifier == deviceInfo.DeviceIdentifier && t.RevokedOn == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedOn = DateTime.UtcNow;
                token.RevokedReason = "User logout";
                _context.AspNetRefreshTokens.Update(token);
            }

            // Update device status
            device.IsActive = false;
            //device.LastLogout = DateTime.UtcNow;
            _context.UserDevices.Update(device);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user from device");
            throw;
        }
    }

    /// <summary>
    /// Gets all active sessions for a user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>List of active sessions with device and location information</returns>
    public async Task<List<SessionInfo>> GetActiveSessionsAsync(int userId)
    {
        try
        {
            var activeSessions = await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .Select(d => new SessionInfo
                {
                    DeviceId = d.DeviceIdentifier,
                    DeviceName = d.DeviceName,
                    DeviceType = d.DeviceType,
                    Browser = d.Browser,
                    OS = d.Os,
                    IpAddress = d.IpAddress,
                    LastActivity = d.LastLogin,
                    HasActiveToken = _context.AspNetRefreshTokens
                        .Any(t => t.DeviceIdentifier == d.DeviceIdentifier && t.RevokedOn == null)
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active sessions for user {UserId}", activeSessions.Count, userId);
            return activeSessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Terminates all sessions except the current one
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="currentDeviceId">ID of the current device to keep active</param>
    public async Task TerminateOtherSessionsAsync(int userId)
    {
        try
        {
            var deviceInfo = _deviceInfoService.GetDeviceInfo();
            // Get all active devices except current one
            var devicesToTerminate = await _context.UserDevices
                .Where(d => d.UserId == userId && d.DeviceIdentifier != deviceInfo.DeviceIdentifier && d.IsActive)
                .ToListAsync();

            foreach (var device in devicesToTerminate)
            {
                // Revoke all refresh tokens for this device
                var tokens = await _context.AspNetRefreshTokens
                    .Where(t => t.DeviceIdentifier == device.DeviceIdentifier && t.RevokedOn == null)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.RevokedOn = DateTime.UtcNow;
                    token.RevokedReason = "All other sessions terminated by user";
                    _context.AspNetRefreshTokens.Update(token);
                }

                // Update device status
                device.IsActive = false;
                //device.LastLogout = DateTime.UtcNow;
                _context.UserDevices.Update(device);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Terminated {Count} other sessions for user {UserId}", devicesToTerminate.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating other sessions for user {UserId}", userId);
            throw;
        }
    }
}