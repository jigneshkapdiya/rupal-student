using System.ComponentModel.DataAnnotations.Schema;
using RupalStudentCore8App.Server.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.Models
{
    public class ItemViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Barcode { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Vat { get; set; }
        public IFormFile ImagePath { get; set; }
        public int SchoolLevelId { get; set; }
    }

    public class SchoolLevelItemFilterModel : PageModel
    {
        public int SchoolLevelId { get; set; }
    }
}
