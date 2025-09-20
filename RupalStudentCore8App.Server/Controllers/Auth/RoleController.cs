using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoleController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RoleManager<AspNetRole> _roleManager;
        private readonly ApplicationDbContext _Db;

        public RoleController(RoleManager<AspNetRole> roleManager,
            ApplicationDbContext Db)
        {
            _Db = Db;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRole()
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.Roles.ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }


        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.Roles.Where(w => w.Id == id).FirstOrDefaultAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(RoleViewModel roleViewModel)
        {
            IdentityResult result;
            try
            {
                #region Create Role
                AspNetRole role = new AspNetRole
                {
                    Name = roleViewModel.Name.Trim(),
                    NormalizedName = roleViewModel.Name.Trim().ToUpper(),
                };
                result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description).ToArray());
                #endregion

                return Ok(role.Id);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRole(RoleViewModel roleViewModel)
        {
            IdentityResult result;
            try
            {
                AspNetRole? role = await _roleManager.FindByIdAsync(roleViewModel.Id.ToString());
                if (role == null)
                    return BadRequest("Role not found.");
                role.Name = roleViewModel.Name.Trim();
                role.NormalizedName = roleViewModel.Name.Trim().ToUpper();
                result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description).ToArray());
                return Ok(role.Id);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    AspNetRole? role = await db.Roles.FindAsync(id);
                    if (role != null)
                    {
                        db.Remove(role);
                        await db.SaveChangesAsync();
                        return Ok(true);
                    }
                    else
                    {
                        return BadRequest(Msg.DeleteRecordNotExists);
                    }
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
    }
}
