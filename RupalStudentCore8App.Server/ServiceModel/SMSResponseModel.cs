namespace RupalStudentCore8App.Server.ServiceModels
{
    public class SMSBodyModel
    {
        public List<long> recipients { get; set; }
        public string body { get; set; }
        public string sender { get; set; }
    }
    public class SMSResponseModel
    {
        public bool isException { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
        public long messageId { get; set; }
        public decimal cost { get; set; }
        public string currency { get; set; }
        public int totalCount { get; set; }
        public int msgLength { get; set; }
        public string accepted { get; set; }
        public string rejected { get; set; }
    }

    public class EmailRequestModel
    {
        public int AppointmentId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string AppointmentNo { get; set; }
        public string SupervisorEmail { get; set; }
        public string SupervisorName { get; set; }
        public string SalespersonEmail { get; set; }
        public string SalespersonName { get; set; }
        public string BranchName { get; set; }
        public string CityName { get; set; }
        public List<TechnicianEmail> TechnicianEmails { get; set; }
    }
    public class TechnicianEmail
    {
        public string Email { get; set; }
        public string FullName { get; set; }
    }
    public class EmailResponseModel
    {
        public bool hasError { get; set; }
        public string message { get; set; }
    }
    public class SMSRequestModel
    {
        public int Id { get; set; }
        public string SenderContactNo { get; set; }
        public string StudentName { get; set; }
        public string AcademicYear { get; set; }
        public Guid guid { get; set; }
    }

}
