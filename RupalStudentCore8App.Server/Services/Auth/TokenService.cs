using RupalStudentCore8App.Server.Class.Auth;
using RupalStudentCore8App.Server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// Provides token generation and validation services for the authentication system.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a cryptographically secure random token for 2FA session management.
    /// </summary>
    /// <returns>A Base64-encoded 256-bit random token.</returns>
    string GenerateSecureToken();

    /// <summary>
    /// Generates a JWT access token for the specified user with their claims and roles.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>A JWT token string containing the user's claims and roles.</returns>
    Task<string> GenerateAccessToken(AspNetUser user);

    /// <summary>
    /// Generates a cryptographically secure refresh token for JWT authentication.
    /// </summary>
    /// <returns>A Base64-encoded 256-bit random token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Extracts and validates the ClaimsPrincipal from an expired JWT token.
    /// </summary>
    /// <param name="token">The expired JWT token to validate.</param>
    /// <returns>A ClaimsPrincipal containing the user's claims if the token is valid.</returns>
    /// <exception cref="SecurityTokenException">Thrown when the token is invalid or uses an unsupported algorithm.</exception>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

/// <summary>
/// Implements token generation and validation services for JWT-based authentication and 2FA.
/// </summary>
/// <remarks>
/// This service handles:
/// - JWT access token generation with user claims
/// - Refresh token generation
/// - 2FA session token generation
/// - Token validation and principal extraction
/// </remarks>
public class TokenService : ITokenService
{
    /// <summary>Application configuration for accessing app settings</summary>
    private readonly IConfiguration _configuration;
    
    /// <summary>JWT configuration containing token secrets and expiration settings</summary>
    private readonly AuthenticationConfiguration _jwtConfig;
    
    /// <summary>Identity user manager for user operations and claims</summary>
    private readonly UserManager<AspNetUser> _userManager;
    
    /// <summary>Identity role manager for role operations</summary>
    private readonly RoleManager<AspNetRole> _roleManager;


    /// <summary>
    /// Initializes a new instance of the TokenService.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="jwtConfig">JWT authentication configuration options.</param>
    /// <param name="userManager">ASP.NET Core Identity user manager.</param>
    /// <param name="roleManager">ASP.NET Core Identity role manager.</param>
    public TokenService(
        IConfiguration configuration,
        IOptions<AuthenticationConfiguration> jwtConfig,
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager
        )
    {
        _configuration = configuration;
        _jwtConfig = jwtConfig.Value;
        _userManager = userManager;
        _roleManager = roleManager;
    }




    /// <summary>
    /// Generates a cryptographically secure random token for 2FA session management.
    /// </summary>
    /// <returns>A Base64-encoded 256-bit random token.</returns>
    public string GenerateSecureToken()
    {
        return GenerateRandomToken();
    }


    /// <summary>
    /// Generates a JWT access token for the specified user with their claims and roles.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>A JWT token string containing the user's claims and roles.</returns>
    /// <remarks>
    /// The token includes:
    /// - Standard user claims (Id, UserName, Email)
    /// - User roles
    /// - Custom claims from the user
    /// - Configured expiration time
    /// - 5-second clock skew allowance
    /// </remarks>
    public async Task<string> GenerateAccessToken(AspNetUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtConfig.AccessTokenSecret);
        var claims = await GetUserClaimsAsync(user);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            NotBefore = DateTime.UtcNow.AddSeconds(-5) // Allow 5 second clock skew
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Retrieves all claims for a user, including their roles and standard claims.
    /// </summary>
    /// <param name="user">The user to get claims for.</param>
    /// <returns>A list of claims associated with the user.</returns>
    private async Task<List<Claim>> GetUserClaimsAsync(AspNetUser user)
    {
        var claims = new ConcurrentBag<Claim>();

        // Get user roles in one call
        var roleNames = await _userManager.GetRolesAsync(user)
            ?? throw new InvalidOperationException("User has no assigned roles");

        // Parallel processing of role claims
        await Parallel.ForEachAsync(roleNames, async (roleName, ct) =>
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                // Get all claims for the role in one call
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims)
                {
                    if (!string.IsNullOrEmpty(claim.Value))
                    {
                        claims.Add(claim);
                    }
                }
            }
        });

        // Add user-specific claims (only if value is not empty/null)
        var userClaims = new List<Claim>();

        AddClaimIfNotEmpty(userClaims, ClaimTypes.NameIdentifier, user.Id.ToString());
        AddClaimIfNotEmpty(userClaims, ClaimTypes.Name, user.UserName);
        AddClaimIfNotEmpty(userClaims, ClaimTypes.Email, user.Email);
        AddClaimIfNotEmpty(userClaims, ClaimTypes.GivenName, user.FullName);

        // jti used for unique token identifier
        userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        // Add issued at claim
        userClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
        // Add 2FA status
        userClaims.Add(new Claim("amr", user.TwoFactorEnabled ? "mfa" : "pwd"));

        // Merge and deduplicate claims
        return claims
            .Union(userClaims)
            .GroupBy(c => new { c.Type, c.Value })
            .Select(g => g.First())
            .ToList();
    }

    private void AddClaimIfNotEmpty(List<Claim> claims, string type, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            claims.Add(new Claim(type, value));
        }
    }


    /// <summary>
    /// Generates a cryptographically secure refresh token for JWT authentication.
    /// </summary>
    /// <returns>A Base64-encoded 256-bit random token.</returns>
    public string GenerateRefreshToken()
    {
        return GenerateRandomToken();
    }

    /// <summary>
    /// Generates a cryptographically secure random token using RNGCryptoServiceProvider.
    /// </summary>
    /// <returns>A Base64-encoded 256-bit random token.</returns>
    private string GenerateRandomToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Extracts and validates the ClaimsPrincipal from an expired JWT token.
    /// </summary>
    /// <param name="token">The expired JWT token to validate.</param>
    /// <returns>A ClaimsPrincipal containing the user's claims if the token is valid.</returns>
    /// <exception cref="SecurityTokenException">Thrown when the token is invalid or uses an unsupported algorithm.</exception>
    /// <remarks>
    /// This method validates the token's signature and format but ignores its expiration time.
    /// It's specifically designed for refresh token scenarios where we need to extract user information
    /// from an expired access token.
    /// </remarks>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,  // Skip audience validation for refresh scenarios
            ValidateIssuer = false,    // Skip issuer validation for refresh scenarios
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.AccessTokenSecret)),
            ValidateLifetime = false    // Skip lifetime validation as we expect an expired token
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            // Ensure the token is a valid JWT and uses the correct algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("The token is not a valid JWT token or does not use HMAC-SHA256 signing");
            }

            return principal;
        }
        catch (SecurityTokenException)
        {
            throw; // Rethrow SecurityTokenException as is
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"Failed to validate token: {ex.Message}", ex);
        }
    }
}

