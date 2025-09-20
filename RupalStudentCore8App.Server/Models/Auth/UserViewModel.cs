using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.Models.Auth
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [MaxLength(256, ErrorMessage = "Role Name must be no longer than 256 characters.")]
        public string RoleName { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name must be no longer than 100 characters.")]
        public string FullName { get; set; }
        [MaxLength(100, ErrorMessage = "Full name Arabic must be no longer than 100 characters.")]
        public string FullNameAr { get; set; }

        //[Required(ErrorMessage = "User name is required.")]
        [MaxLength(256, ErrorMessage = "User name must be no longer than 60 characters.")]
        public string UserName { get; set; }
        [StringLength(15)]
        public string PhoneCode { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(256, ErrorMessage = "Email must be no longer than 60 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        //[MaxLength(9, ErrorMessage = "Phone Number must be no longer than 9 characters.")]
        public string PhoneNumber { get; set; }

        public bool Status { get; set; }
        [MaxLength(50, ErrorMessage = "Password must be no longer than 50 characters.")]
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [change password required].
        /// </summary>
        public bool ChangePasswordRequired { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }

    public class UserFilterModel : PageModel
    {
        public string RoleName { get; set; }
        public int RoleId { get; set; }
    }

}
