namespace RupalStudentCore8App.Server.Models
{
    public class AutoIncrementViewModel
    {
        public int Id { get; set; }
        public int IncrementId { get; set; }
        public string Prefix { get; set; }
        public int PadNumber { get; set; }
        public string Entity { get; set; }
        public string Description { get; set; }
    }
}
