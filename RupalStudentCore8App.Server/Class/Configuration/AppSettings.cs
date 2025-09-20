namespace RupalStudentCore8App.Server.Class.Configuration;

public class AppSettings
{
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public GoogleSettings Google { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public SmsSettings SMS { get; set; } = new();
    public TelrSettings Telr { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; }
}

public class AuthenticationSettings
{
    public string AccessTokenSecret { get; set; }
    public string RefreshTokenSecret { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public int OtpResendCooldownSeconds { get; set; }
    public int MaxDailyOtpAttempts { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AllowedDevices { get; set; }
    public bool Enable2FA { get; set; }
    public string WebAppUrl { get; set; }
}

public class GoogleSettings
{
    public string ClientId { get; set; }
    public ReCaptchaSettings ReCaptcha { get; set; } = new();
}

public class ReCaptchaSettings
{
    public string SecretKey { get; set; }
}

public class EmailSettings
{
    public bool AllowSending { get; set; }
    public SmtpSettings Smtp { get; set; } = new();
    public FromMailAddressSettings DefaultFrom { get; set; } = new();
}

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class FromMailAddressSettings
{
    public string Address { get; set; }
    public string DisplayName { get; set; }
}

public class SmsSettings
{
    public bool AllowSending { get; set; }
    public string ApiUrl { get; set; }
    public string Token { get; set; }
    public string SenderName { get; set; }
}

public class TelrSettings
{
    public int Store { get; set; }
    public string AuthKey { get; set; }
    public string ApiUrl { get; set; }
    public string TestMode { get; set; }
    public string AuthorizedUrl { get; set; }
    public string DeclinedUrl { get; set; }
    public string CancelledUrl { get; set; }
    public TelrCustomerSettings Customer { get; set; } = new();
    public TelrBookingSettings Booking { get; set; } = new();
}

public class TelrCustomerSettings
{
    public string DefaultEmail { get; set; }
    public string AddressLine1 { get; set; }
    public string City { get; set; }
    public string CountryCode { get; set; }
    public string EmailDomain { get; set; }
}

public class TelrBookingSettings
{
    public bool PreventBooking { get; set; }
    public string PreventBookingMessage { get; set; }
}

public class LoggingSettings
{
    public Dictionary<string, string> LogLevel { get; set; } = new();
}
