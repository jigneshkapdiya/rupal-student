using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.Models.Auth
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string FullName { get; set; }
        [StringLength(100)]
        public string FullNameAr { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(256)]
        public string PhoneNumber { get; set; }
        public IFormFile ProfileImage { get; set; }
    }
}
