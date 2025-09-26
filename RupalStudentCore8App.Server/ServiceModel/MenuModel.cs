using System.ComponentModel.DataAnnotations;

namespace RupalStudentCore8App.Server.ServiceModels
{
    public class MenuModel
    {
        public int Id { get; set; }
        public int OrderNo { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Icon { get; set; }
        // public string Routes { get; set; }
        public string Path { get; set; }
        public int ParentId { get; set; }
        public bool Selected { get; set; }
        public bool IsExternalLink { get; set; }
        //public string BadgeClass { get; set; }
        //public string Badge { get; set; }
        public string Class { get; set; }
        public List<MenuModel> Submenu { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }

    public class MenuPermissionModel
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int RoleId { get; set; }
    }

    public class PermissionModel
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Description_Ar { get; set; }
        public bool Selected { get; set; }
    }

    public class RolePermissionModel
    {
        public int Id { get; set; }
        public int RoleId
        {
            get; set;
        }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be 8 to 20 character long")]
        [MaxLength(20, ErrorMessage = "Password must be 8 to 20 character long")]
        public required string NewPassword { get; set; }

        public int Id { get; set; }
    }
}
