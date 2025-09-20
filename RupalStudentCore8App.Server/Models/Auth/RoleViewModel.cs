using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.Models.Auth
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string Name { get; set; }
    }
}
