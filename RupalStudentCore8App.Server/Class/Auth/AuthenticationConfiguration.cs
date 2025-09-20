namespace RupalStudentCore8App.Server.Class.Auth
{
    public class AuthenticationConfiguration
    {
        public string AccessTokenSecret { get; set; }
        public double AccessTokenExpirationMinutes { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string RefreshTokenSecret { get; set; }
        public double RefreshTokenExpirationMinutes { get; set; }
        public int AllowedDevices { get; set; }
        public int OtpResendCooldownSeconds { get; set; }
        public int MaxDailyOtpAttempts { get; set; }
    }
}
