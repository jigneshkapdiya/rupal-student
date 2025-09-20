namespace RupalStudentCore8App.Server.Models.Auth
{
    public class LoginHistoryViewModal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; }
        public bool Success { get; set; }
    }
}
