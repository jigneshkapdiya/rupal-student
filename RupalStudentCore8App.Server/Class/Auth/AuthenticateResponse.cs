namespace RupalStudentCore8App.Server.Class
{

    public class AuthenticateResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public bool PhoneNumberConfirmed { get; set; }


    }
}
