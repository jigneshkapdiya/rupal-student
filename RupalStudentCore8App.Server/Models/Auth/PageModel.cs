namespace RupalStudentCore8App.Server.Models.Auth
{
    public class PageModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public bool? Status { get; set; }
    }
}
