using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.Services;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MenuController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;
        private readonly IPermissionService _permissionService;

        public MenuController(ApplicationDbContext Db)
        {
            _Db = Db;
            _permissionService = new PermissionService();
        }

        /// <summary>
        /// Get Menu by ParentId
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpGet("GetList")]
        public async Task<IActionResult> GetList(int parentId)
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.AspMenus.Where(w => w.ParentId == parentId).OrderBy(o => o.OrderNo).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Menu get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetMenuById(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.AspMenus.Where(w => w.Id == id).FirstOrDefaultAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Name")]
        public async Task<IActionResult> GetName()
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.AspMenus.Where(w => w.Class.Contains("has-sub")).Select(s => new
                    {
                        s.Id,
                        s.Name,
                    }).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);

            }
        }

        /// <summary>
        /// Menu add and update
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddUpdateMenu(AspMenu vm)
        {
            try
            {
                using (var db = _Db)
                {
                    AspMenu? entity;
                    if (vm.Id > 0)
                    {
                        entity = db.AspMenus.Find(vm.Id);
                        if (entity == null)
                            return BadRequest("No record found to update.");

                        entity.Name = vm.Name;
                        entity.Title = vm.Title;
                        entity.TitleAr = vm.TitleAr;
                        entity.Path = vm.Path;
                        entity.Class = vm.Class;
                        entity.Icon = vm.Icon;
                        entity.ParentId = vm.ParentId;
                        entity.IsExternalLink = vm.IsExternalLink;
                        db.AspMenus.Update(entity);
                        _ = await db.SaveChangesAsync();
                        return Ok(entity.Id);

                    }
                    else
                    {
                        entity = new AspMenu
                        {
                            Name = vm.Name,
                            Title = vm.Title,
                            TitleAr = vm.TitleAr,
                            Path = vm.Path,
                            Class = vm.Class,
                            Icon = vm.Icon,
                            ParentId = vm.ParentId,
                            IsExternalLink = vm.IsExternalLink,
                            OrderNo = 1 + db.AspMenus.Where(w => w.ParentId == vm.ParentId).OrderByDescending(w => w.OrderNo).Select(s => s.OrderNo).FirstOrDefault()
                        };
                        db.AspMenus.Add(entity);
                        _ = await db.SaveChangesAsync();
                        return Ok(entity.Id);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Menu delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    AspMenu? entity = await db.AspMenus.FindAsync(id);
                    if (entity != null)
                    {
                        if (db.AspMenus.Any(w => w.ParentId == entity.Id))
                        {
                            return BadRequest("There are submenus in this menu, so you can't delete it!");
                        }
                        db.AspMenus.Remove(entity);
                        _ = await db.SaveChangesAsync();
                        return Ok(entity.Id);
                    }
                    else
                    {
                        return BadRequest("No record found to delete");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// permission getlist
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpGet("PermissionByMenu/{menuId}")]
        public async Task<IActionResult> GetPermissionList(int menuId)
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.AspNetPermissions.Where(w => w.MenuId == menuId).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Permission get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Permission/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.AspNetPermissions.Where(w => w.Id == id).FirstOrDefaultAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Permission add and update
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost("Permission")]
        public async Task<IActionResult> AddUpdatePermission(AspNetPermission vm)
        {
            try
            {
                using (var db = _Db)
                {
                    AspNetPermission? entity;
                    if (vm.Id > 0)
                    {
                        entity = db.AspNetPermissions.Find(vm.Id);
                        if (entity == null)
                            return BadRequest("No record found to update.");

                        entity.Name = vm.Name;
                        entity.DescriptionAr = vm.DescriptionAr;
                        entity.Description = vm.Description;
                        entity.MenuId = vm.MenuId;
                        db.AspNetPermissions.Update(entity);
                        _ = await db.SaveChangesAsync();
                        return Ok(entity.Id);

                    }
                    else
                    {
                        entity = new AspNetPermission
                        {
                            Name = vm.Name,
                            DescriptionAr = vm.DescriptionAr,
                            Description = vm.Description,
                            MenuId = vm.MenuId
                        };
                        db.AspNetPermissions.Add(entity);
                        _ = await db.SaveChangesAsync();
                        return Ok(entity.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Permission delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("Permission/{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    AspNetPermission? entity = await db.AspNetPermissions.FindAsync(id);
                    if (entity == null)
                        return BadRequest(Msg.DeleteRecordNotExists);

                    db.AspNetPermissions.Remove(entity);
                    _ = await db.SaveChangesAsync();
                    return Ok(entity.Id);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                if (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
                {
                    return BadRequest(Msg.DeleteRefersToOther);
                }
                else
                {
                    return BadRequest(Msg.DeleteError);
                }

            }
        }

        [HttpPost("ChangeOrder")]
        public async Task<IActionResult> ChangeOrder(MenuOrderModel vm)
        {
            try
            {
                return Ok(await _permissionService.ChangeOrder(vm));
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);

            }
        }
    }
}
