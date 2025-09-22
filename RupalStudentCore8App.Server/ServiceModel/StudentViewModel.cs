using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RupalStudentCore8App.Server.ServiceModel
{
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string Mobile { get; set; }
        public string FamilyName { get; set; }
        public string FamilyNameGu { get; set; }
        public string StudentName { get; set; }
        public string StudentNameGu { get; set; }
        public string FatherName { get; set; }
        public string FatherNameGu { get; set; }
        public string Education { get; set; }
        public string EducationGu { get; set; }
        public string SchoolName { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Sgpa { get; set; }
        public decimal? Cgpa { get; set; }
        public string AcademicYear { get; set; }
        public string Status { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; }
    }

    public class AttachmentViewModel
    {
        public IFormFile File { get; set; }
        public string FileName { get; set; }

        [StringLength(50)]
        public string ReferenceType { get; set; }
        public string FileUrl { get; set; }

        [StringLength(300)]
        public string Description { get; set; }
    }

}
