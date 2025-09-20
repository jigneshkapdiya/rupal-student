using RupalStudentCore8App.Server.ServiceModels;

namespace RupalStudentCore8App.Server.Models.Auth
{
    public class MenuViewModel
    {
        public int RoleId { get; set; }
        public List<int> MenuId { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }

    public class MenuOrderModel
    {
        public int MenuId { get; set; }
        public int ParentId { get; set; }
        public int NewOrder { get; set; }
        public int PrevOrder { get; set; }
    }
}
