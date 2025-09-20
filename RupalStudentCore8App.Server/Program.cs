using RupalStudentCore8App.Server.Class;
using RupalStudentCore8App.Server.Class.Auth;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Log4Net;
using RupalStudentCore8App.Server.Services;
using RupalStudentCore8App.Server.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static RupalStudentCore8App.Server.Class.GlobalConstant;
using Utility = RupalStudentCore8App.Server.Services.Utility;
using RupalStudentCore8App.Server.Class.Configuration;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

//
// Configure Core Services
//

builder.Services.AddControllers();

// Register HttpClientFactory and HttpContextAccessor for DI
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Configure strongly-typed application settings
builder.Services.AddAppConfiguration(configuration);
// Get the connection string once and reuse it for consistency
var defaultConnection = configuration.GetConnectionString("DefaultConnection");

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(defaultConnection);
});

//
// Configure Authentication Token Providers
//

// Provider for email verification tokens
var emailTokenProviderType = typeof(EmailTokenProvider<AspNetUser>);

// Provider for phone verification tokens
var phoneTokenProviderType = typeof(PhoneNumberTokenProvider<AspNetUser>);

// Provider for two-factor authentication session tokens
var twoFactorTokenProviderType = typeof(TwoFactorSessionTokenProvider<AspNetUser>);

//
// Configure ASP.NET Core Identity
//

// Set up Identity with custom user and role types
// Note: Password requirements are intentionally relaxed for development
// TODO: Consider strengthening password requirements for production
builder.Services.AddIdentity<AspNetUser, AspNetRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;           // Don't require numbers
    options.Password.RequireUppercase = false;       // Don't require uppercase
    options.Password.RequireLowercase = false;       // Don't require lowercase
    options.Password.RequireNonAlphanumeric = false; // Don't require special chars
    options.Password.RequiredLength = 6;             // Minimum length of 6
    options.Password.RequiredUniqueChars = 0;        // No unique chars required
})
    // Configure storage and providers
    .AddEntityFrameworkStores<ApplicationDbContext>()  // Use EF Core for identity storage
    .AddDefaultTokenProviders()                       // Add standard token providers
    .AddTokenProvider("EmailCode", emailTokenProviderType)           // For email verification
    .AddTokenProvider("PhoneCode", phoneTokenProviderType)           // For SMS verification
    .AddTokenProvider(LoginProvider.TwoFactor, twoFactorTokenProviderType); // For 2FA session management

//
// API Documentation Setup
//

// Enable API endpoint discovery
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication support
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
});

// Configure database connections
Config.DefaultConnectionString = defaultConnection;
Config.GpConnectionString = configuration.GetConnectionString("GpConnection");

// Configure JWT Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var authConfig = builder.Configuration.GetSection("Authentication").Get<AuthenticationSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authConfig.Issuer,
            ValidAudience = authConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authConfig.AccessTokenSecret)
            ),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Override the default redirect behavior
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
                return context.Response.WriteAsync(result);
            }
        };
    });

// Core services
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache(); // Add distributed cache for OTP rate limiting
builder.Services.AddScoped<IUtility, Utility>();
builder.Services.AddScoped<IdentityDataInitializer>();

// Authentication services
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IDeviceInfoService, DeviceInfoService>();

//
// Communication Services
//
builder.Services.AddTransient<IEmailService, EmailService>();        // Email notifications
builder.Services.AddTransient<ISMSService, SMSService>();           // SMS notifications

//
// Configuration Binding
//

// Bind authentication settings from appsettings.json
// This provides strongly-typed access to authentication configuration
builder.Services.Configure<AuthenticationConfiguration>(
    builder.Configuration.GetSection("Authentication"));

//
// CORS Configuration
//

// Configure Cross-Origin Resource Sharing (CORS)
// WARNING: Current configuration is permissive for development
// TODO: For production, replace with specific origins:
// .WithOrigins("https://your-domain.com")
// .AllowCredentials()
// .WithMethods("GET", "POST", "PUT", "DELETE")
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder
        .AllowAnyHeader()      // Allow any HTTP header
        .AllowAnyMethod()      // Allow any HTTP method
        .SetIsOriginAllowed((host) => true)  // Allow any origin (development only)
        .AllowCredentials();   // Allow sending of auth credentials
}));

//
// Localization Configuration
//

// Configure resource path for localization files
builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");

// Build the application
var app = builder.Build();

// Define supported cultures (English and Arabic)
var supportedCultures = new[] { "en", "ar" };

// Configure request localization
app.UseRequestLocalization(options =>
{
    // Customize culture provider order:
    // 1. QueryString (?culture=en-US)
    // 2. Accept-Language header
    // 3. Cookie
    var acceptLanguageCultureProvider = options.RequestCultureProviders[2];
    options.RequestCultureProviders.RemoveAt(0);
    options.RequestCultureProviders.Insert(1, acceptLanguageCultureProvider);

    // Add culture info to response headers for client awareness
    options.ApplyCurrentCultureToResponseHeaders = true;

    // Configure supported cultures for both server and UI
    options.AddSupportedCultures(supportedCultures);     // For model binding and validation
    options.AddSupportedUICultures(supportedCultures);  // For UI string localization
});

//
// Logging Configuration
//

// Configure Log4Net provider
var logProvider = app.Services.GetRequiredService<ILoggerFactory>();
logProvider.AddProvider(new Log4NetProvider("log4net.config"));

// Configure the HTTP request pipeline.
// Initialize Database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var dataInitializer = services.GetRequiredService<IdentityDataInitializer>();
        await dataInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw; // Re-throw as this is a critical error
    }
}

//
// Security Headers Configuration
//

// Add security headers as early as possible in the pipeline
// These headers help protect against various web vulnerabilities
app.Use(async (context, next) =>
{
    // Basic security headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");           // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Frame-Options", "DENY");                     // Prevent clickjacking
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");          // Enable browser XSS protection
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin"); // Control referrer info
    
    // Restrict access to browser features
    context.Response.Headers.Append("Permissions-Policy", 
        "accelerometer=(), camera=(), geolocation=(), microphone=()");           // Limit sensitive APIs
    
    // Content Security Policy (CSP)
    // Carefully configured to allow necessary resources while maintaining security
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; " +                // Only allow resources from same origin
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +  // Required for some UI frameworks
        "style-src 'self' 'unsafe-inline';");   // Allow inline styles for flexibility

    await next();
});

//
// Environment-Specific Configuration
//

// Development environment setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // Enable API documentation
    app.UseSwaggerUI(); // Serve Swagger UI for API testing
}
// Production environment setup
else
{
    app.UseHsts(); // Enable HTTP Strict Transport Security for enhanced security
    app.UseExceptionHandler("/Error");                    // Global error handling
    app.UseStatusCodePagesWithReExecute("/Error/{0}");    // Custom error pages for status codes
}

//
// Core Middleware Pipeline
//

// Security and protocol handling
app.UseHttpsRedirection();                // Redirect HTTP to HTTPS
app.UseCors("CorsPolicy");               // Apply CORS policy

// Static file handling with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year (31536000 seconds)
        // This improves performance for static assets
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    }
});

// Routing and Authentication
app.UseRouting();                         // Enable endpoint routing
app.UseAuthentication();                  // Enable authentication
app.UseAuthorization();                   // Enable authorization

//
// Endpoint Configuration
//
app.MapControllers();

// SPA fallback - serves index.html for unmatched routes
// This enables client-side routing for SPAs
app.MapFallbackToFile("/index.html");

app.Run();
