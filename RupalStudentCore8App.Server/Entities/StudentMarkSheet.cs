using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RupalStudentCore8App.Server.Entities
{
    [Table("StudentMarkSheet")]
    public partial class StudentMarkSheet
    {
        [Key]
        public int Id { get; set; }
        [StringLength(10)]
        public string? FormNumber { get; set; }

        [Required]
        [StringLength(15)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(50)]
        public string FamilyName { get; set; }

        [StringLength(50)]
        public string FamilyNameGu { get; set; }

        [StringLength(50)]
        public string StudentName { get; set; }

        [StringLength(50)]
        public string StudentNameGu { get; set; }

        [StringLength(50)]
        public string FatherName { get; set; }

        [StringLength(50)]
        public string FatherNameGu { get; set; }

        [Required]
        [StringLength(50)]
        public string Education { get; set; }

        [StringLength(50)]
        public string EducationGu { get; set; }

        [StringLength(50)]
        public string SchoolName { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Percentage { get; set; }

        [Column("SGPA", TypeName = "decimal(5, 2)")]
        public decimal? Sgpa { get; set; }

        [Column("CGPA", TypeName = "decimal(5, 2)")]
        public decimal? Cgpa { get; set; }

        [StringLength(10)]
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        [StringLength(20)]
        public string Grade { get; set; }
        [StringLength(4000)]

        public string Description { get; set; }
        [StringLength(10)]
        public string SequenceNumber { get; set; }
        [StringLength(10)]
        public string GroupSequenceNumber { get; set; }
        public DateTime? CreatedOn { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
    }
}
