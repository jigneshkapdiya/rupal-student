using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RupalStudentCore8App.Server.Entities
{
    [Table("StudentEducation")]
    public partial class StudentEducation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string NameGu { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SequenceNo { get; set; }
    }
}
