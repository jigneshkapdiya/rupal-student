using RupalStudentCore8App.Server.Class;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.ServiceModels;
using Dapper;
using System.Data;

namespace RupalStudentCore8App.Server.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<MenuModel>> GetMenuByRoleId(int roleId);
        Task<IEnumerable<MenuModel>> GetMenuByRoleName(string roleName);
        Task<IEnumerable<RolePermissionModel>> GetRolePermissionsByRoleId(int roleId);
        Task<int> ChangeOrder(MenuOrderModel vm);
        Task<IEnumerable<MenuModel>> GetClubMenuByRoleId(int roleId);
        Task<IEnumerable<MenuModel>> GetClubMenuByRoleName(string roleName);
    }

    public class PermissionService : IPermissionService
    {
        public async Task<IEnumerable<MenuModel>> GetMenuByRoleId(int roleId)
        {
            IEnumerable<MenuModel> menuModels;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@p_RoleId", roleId, DbType.String);
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                menuModels = await db.QueryAsync<MenuModel>("SP_MenuPermissions_Get", dynamicParameters, null, null, CommandType.StoredProcedure);
            }
            return menuModels;
        }

        public async Task<IEnumerable<MenuModel>> GetMenuByRoleName(string roleName)
        {
            IEnumerable<MenuModel> menuModels;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@p_RoleName", roleName, DbType.String);
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                menuModels = await db.QueryAsync<MenuModel>("SP_MenuPermissions_GetBy_RoleName", dynamicParameters, null, null, CommandType.StoredProcedure);
            }
            return menuModels;
        }

        public async Task<IEnumerable<RolePermissionModel>> GetRolePermissionsByRoleId(int roleId)
        {
            IEnumerable<RolePermissionModel> list;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@p_RoleId", roleId, DbType.String);
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                list = await db.QueryAsync<RolePermissionModel>("SP_RolePermissions_Get", dynamicParameters, null, null, CommandType.StoredProcedure);
            }
            return list;
        }

        public async Task<int> ChangeOrder(MenuOrderModel vm)
        {
            int result = 0;
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                result = await db.ExecuteScalarAsync<int>("SP_Menu_ChangeOrder",
                    new
                    {
                        p_ParentId = vm.ParentId,
                        p_MenuId = vm.MenuId,
                        p_NewOrder = vm.NewOrder,
                        p_PrevOrder = vm.PrevOrder,
                    }, null, null, CommandType.StoredProcedure);
            }
            return result;
        }

        public async Task<IEnumerable<MenuModel>> GetClubMenuByRoleId(int roleId)
        {
            IEnumerable<MenuModel> menuModels;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@p_RoleId", roleId, DbType.String);
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                menuModels = await db.QueryAsync<MenuModel>("SP_ClubMenuPermissions_Get", dynamicParameters, null, null, CommandType.StoredProcedure);
            }
            return menuModels;
        }

        public async Task<IEnumerable<MenuModel>> GetClubMenuByRoleName(string roleName)
        {
            IEnumerable<MenuModel> menuModels;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@p_RoleName", roleName, DbType.String);
            using (IDbConnection db = new DbFactory(Config.DefaultConnectionString).db)
            {
                menuModels = await db.QueryAsync<MenuModel>("SP_ClubMenuPermissions_GetBy_RoleName", dynamicParameters, null, null, CommandType.StoredProcedure);
            }
            return menuModels;
        }

    }
}
