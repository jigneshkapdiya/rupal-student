using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RupalStudentCore8App.Server.Class.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind strongly-typed settings matching appsettings.* structure
        services.Configure<AppSettings>(configuration);
        services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));
        services.Configure<GoogleSettings>(configuration.GetSection("Google"));
        services.Configure<SmtpSettings>(configuration.GetSection("Email:Smtp"));
        services.Configure<FromMailAddressSettings>(configuration.GetSection("Email:DefaultFrom"));
        services.Configure<SmsSettings>(configuration.GetSection("SMS"));
        services.Configure<TelrSettings>(configuration.GetSection("Telr"));

        return services;
    }
}
