using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.ServiceModels;
using RupalStudentCore8App.Server.Services;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PermissionController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;
        private readonly RoleManager<AspNetRole> _roleManager;
        private readonly IPermissionService _permissionService;
        public PermissionController(ApplicationDbContext Db, RoleManager<AspNetRole> roleManager)
        {
            _Db = Db;
            _roleManager = roleManager;
            _permissionService = new PermissionService();
        }

        [Route("RolePermission")]
        [HttpGet]
        public async Task<IActionResult> GetRolePermission()
        {
            try
            {
                var roleq = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault();
                string role = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault().Value;
                using (var db = _Db)
                {
                    int roleId = await db.Roles.Where(w => w.Name == role).Select(s => s.Id).FirstOrDefaultAsync();
                    return Ok(await db.RoleClaims.Where(w => w.RoleId == roleId).Select(s => new RolePermissionModel
                    {
                        Id = s.Id,
                        Name = s.ClaimValue,
                        RoleId = s.RoleId,
                    }).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [Route("GetMenuList")]
        [HttpGet]
        public async Task<IActionResult> GetMenuListByRoleName()
        {
            try
            {
                bool useArabicName = HttpContext.Request.Headers["Accept-Language"].ToString() == "ar";

                string role = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault().Value;
                List<MenuModel> menuList = (List<MenuModel>)await _permissionService.GetMenuByRoleName(role);
                if (menuList.Count() > 0)
                {
                    menuList = menuList.Where(w => w.ParentId == 0).Select(s => new MenuModel
                    {
                        Id = s.Id,
                        Icon = s.Icon,
                        Name = s.Name,
                        OrderNo = s.OrderNo,
                        ParentId = s.ParentId,
                        Path = s.Path,
                        Class = s.Class,
                        Selected = s.Selected,
                        IsExternalLink = s.IsExternalLink,
                        Title = useArabicName && !string.IsNullOrEmpty(s.TitleAr) ? s.TitleAr : s.Title,
                        Submenu = menuList.Where(sw => sw.ParentId == s.Id).OrderBy(so => so.OrderNo).ToList() == null ? new List<MenuModel>() : menuList.Where(sw => sw.ParentId == s.Id).Select(ss => new MenuModel
                        {
                            Id = ss.Id,
                            Icon = ss.Icon,
                            Name = ss.Name,
                            OrderNo = ss.OrderNo,
                            ParentId = ss.ParentId,
                            Path = ss.Path,
                            Selected = ss.Selected,
                            Class = ss.Class,
                            IsExternalLink = s.IsExternalLink,
                            Title = useArabicName && !string.IsNullOrWhiteSpace(ss.TitleAr) ? ss.TitleAr : ss.Title,
                            Submenu = menuList.Where(ssw => ssw.ParentId == ss.Id).OrderBy(sso => sso.OrderNo).ToList() == null ? new List<MenuModel>() : menuList.Where(ssw => ssw.ParentId == ss.Id).Select(sss => new MenuModel
                            {
                                Id = sss.Id,
                                Icon = sss.Icon,
                                Name = sss.Name,
                                OrderNo = sss.OrderNo,
                                ParentId = sss.ParentId,
                                Path = sss.Path,
                                Selected = sss.Selected,
                                Class = sss.Class,
                                IsExternalLink = sss.IsExternalLink,
                                Title = useArabicName && !string.IsNullOrWhiteSpace(sss.TitleAr) ? sss.TitleAr : sss.Title,
                                Submenu = new List<MenuModel>()
                            }).OrderBy(sso => sso.OrderNo).ToList()
                        }).OrderBy(so => so.OrderNo).ToList()
                    }).OrderBy(o => o.OrderNo).ToList();
                }
                return Ok(menuList);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [Route("MenuList/{roleId}")]
        [HttpGet]
        public async Task<IActionResult> GetMenuList(int roleId = 0)
        {
            try
            {
                string defaultMenu = "Dashboard";
                string defaultClaim = "Dashboard.ViewDashboard";
                bool useArabicName = HttpContext.Request.Headers["Accept-Language"].ToString() == "ar";
                //string lang = HttpContext.Request.Headers["Lang"].ToString();
                IEnumerable<MenuModel> menuList = await _permissionService.GetMenuByRoleId(roleId);
                var roleClaims = _Db.RoleClaims.Where(w => w.RoleId == roleId).ToList();
                return Ok(new
                {
                    data = menuList.Where(w => w.ParentId == 0).Select(s => new
                    {
                        s.Id,
                        s.Icon,
                        s.Name,
                        s.OrderNo,
                        s.ParentId,
                        s.Path,
                        s.Class,
                        Selected = s.Name == defaultMenu ? true : s.Selected,
                        Title = useArabicName && !string.IsNullOrWhiteSpace(s.TitleAr) ? s.TitleAr : s.Title,
                        SubList = menuList.Where(sw => sw.ParentId == s.Id).Select(ss => new
                        {
                            ss.Id,
                            ss.Icon,
                            ss.Name,
                            ss.OrderNo,
                            ss.ParentId,
                            ss.Path,
                            ss.Selected,
                            ss.Class,
                            Title = useArabicName && !string.IsNullOrWhiteSpace(ss.TitleAr) ? ss.TitleAr : ss.Title,
                            SubList = menuList.Where(ssw => ssw.ParentId == ss.Id).Select(sss => new
                            {
                                sss.Id,
                                sss.Icon,
                                sss.Name,
                                sss.OrderNo,
                                sss.ParentId,
                                sss.Path,
                                sss.Class,
                                sss.Selected,
                                Title = useArabicName && !string.IsNullOrWhiteSpace(sss.TitleAr) ? sss.TitleAr : sss.Title,
                                Submenu = new List<MenuModel>(),
                                Permissions = _Db.AspNetPermissions.Where(p => p.MenuId == sss.Id).AsEnumerable().Select(
                                ps => new PermissionModel
                                {
                                    Id = ps.Id,
                                    MenuId = sss.Id,
                                    Name = ps.Name,
                                    Description = useArabicName ? ps.DescriptionAr : ps.Description,
                                    Selected = roleClaims.Where(rc => rc.ClaimValue == ps.Name).ToList().Count > 0 ? true : false
                                }).ToList(), // action permissions
                            }).OrderBy(sso => sso.OrderNo).ToList(),
                            Permissions = _Db.AspNetPermissions.Where(p => p.MenuId == ss.Id).AsEnumerable().Select(
                                ps => new PermissionModel
                                {
                                    Id = ps.Id,
                                    MenuId = ss.Id,
                                    Name = ps.Name,
                                    Description = useArabicName ? ps.DescriptionAr : ps.Description,
                                    Selected = roleClaims.Where(rc => rc.ClaimValue == ps.Name).ToList().Count > 0 ? true : false
                                }).ToList(), // action permissions
                        }).OrderBy(so => so.OrderNo).ToList(),
                        Permissions = _Db.AspNetPermissions.Where(p => p.MenuId == s.Id).AsEnumerable().Select(
                                ps => new PermissionModel
                                {
                                    Id = ps.Id,
                                    MenuId = s.Id,
                                    Name = ps.Name,
                                    Description = useArabicName ? ps.DescriptionAr : ps.Description,
                                    Selected = ps.Name == defaultClaim ? true : roleClaims.Where(rc => rc.ClaimValue == ps.Name).ToList().Count > 0 ? true : false
                                }).ToList(), // action permissions
                    }).OrderBy(o => o.OrderNo).ToList()
                });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePermission(MenuViewModel vm)
        {
            try
            {
                using (var db = _Db)
                {
                    //Remove menu and then add menu by role
                    AspMenuPermission entity;
                    var menuPermissionList = await db.AspMenuPermissions.Where(w => w.RoleId == vm.RoleId).ToListAsync();
                    if (menuPermissionList.Count > 0)
                    {
                        db.RemoveRange(menuPermissionList);
                        await db.SaveChangesAsync();
                    }
                    foreach (int menuId in vm.MenuId)
                    {
                        entity = new AspMenuPermission
                        {
                            MenuId = menuId,
                            RoleId = vm.RoleId
                        };
                        db.Add(entity);
                    }
                    await db.SaveChangesAsync();

                    //Remove claim and then add claims by role
                    AspNetRole? role = await _roleManager.FindByIdAsync(vm.RoleId.ToString());
                    if (role != null)
                    {
                        var roleClaims = await db.RoleClaims.Where(w => w.RoleId == role.Id).ToListAsync();
                        if (roleClaims != null && roleClaims.Count > 0)
                        {
                            db.RemoveRange(roleClaims);
                            await db.SaveChangesAsync();
                        }
                        foreach (PermissionModel claim in vm.Permissions)
                        {
                            string type = claim.Name.Split('.')[0].Trim();
                            await _roleManager.AddClaimAsync(role, new Claim(type, claim.Name.Trim()));
                        }
                    }
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
        }



    }
}
