using RupalStudentCore8App.Server.Class.Configuration;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.Services;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Claims;
using static RupalStudentCore8App.Server.Class.GlobalConstant;
using RupalStudentCore8App.Server.ServiceModels;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        #region Init
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IUtility _utility;

        public UserController(
            ApplicationDbContext Db,
            UserManager<AspNetUser> userManager,
            IOptionsSnapshot<AppSettings> appSettings,
            ILogger<UserController> logger,
            IUtility utility
            )
        {
            _Db = Db;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _logger = logger;
            _utility = utility;
        }
        #endregion

        [HttpPost("GetList")]
        public async Task<IActionResult> GetUser(UserFilterModel vm)
        {
            try
            {
                using (var db = _Db)
                {
                    bool useArabicName = HttpContext.Request.Headers["Accept-Language"].ToString() == "ar";
                    IQueryable<AspNetUser> query = db.Users.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(vm.SearchText))
                    {
                        var search = vm.SearchText.Trim();
                        query = query.Where(w => w.FullName.Contains(search)
                                        || w.FullNameAr.Contains(search)
                                        || w.UserName.Contains(search)
                                        || w.Email.Contains(search)
                                        || w.PhoneNumber.Contains(search)
                                        );
                    }
                    if (vm.Status != null)
                    {
                        query = query.Where(w => w.Status == vm.Status);
                    }
                    var list = await query.Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        s.FullNameAr,
                        s.UserName,
                        s.Email,
                        s.PhoneNumber,
                        s.Status,
                        s.Readonly,
                        s.TwoFactorEnabled,
                        Allow2FA = _appSettings.Authentication.Enable2FA,
                        RoleInfo = (from userrole in db.UserRoles
                                    where userrole.UserId == s.Id
                                    join role in db.Roles on userrole.RoleId equals role.Id
                                    where vm.RoleId == 0 || userrole.RoleId == vm.RoleId
                                    select new { role.Name }
                                  ).FirstOrDefault()
                    }).OrderBy(o => o.RoleInfo.Name).ThenBy(x => x.FullName)
                      .Where(w =>
                                !string.IsNullOrWhiteSpace(w.RoleInfo.Name)
                               )
                      .ToListAsync();
                    int totalRecord = list.Count();
                    list = list.Skip((vm.Page - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                    return Ok(new { dataList = list, totalRecord });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                return Ok(await _Db.Users.Where(w => id == w.Id).Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.FullNameAr,
                    s.UserName,
                    s.Email,
                    s.PhoneNumber,
                    s.Status,
                    s.Readonly,
                    ProfileImage = _utility.GetFileUrl(s.ProfileImage, FileType.ProfileImage),
                    Allow2FA = _appSettings.Authentication.Enable2FA,
                    s.TwoFactorEnabled,
                    RoleName = (from userrole in _Db.UserRoles
                                where userrole.UserId == s.Id
                                join role in _Db.Roles on userrole.RoleId
                                equals role.Id
                                select role.Name).FirstOrDefault()
                }).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel vm)
        {

            int signInUserId = Convert.ToInt32(HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value);

            // check if email is already exists
            if (_Db.Users.Any(w => w.Email == vm.Email))
                return BadRequest($"User with same email '{vm.Email}' already exists. Please try with a new email.");
            // check if PhoneNumber is already exists
            //if (_Db.Users.Any(w => w.PhoneNumber == vm.PhoneNumber && w.PhoneCode == Common.PhoneCode))
            //    return BadRequest($"User with the same phone number '{vm.PhoneNumber}' already exists. Please try with a new phone number.");

            #region 1) Create New User, Table: AspNetUser
            AspNetUser user = new AspNetUser
            {
                FullName = vm.FullName,
                FullNameAr = vm.FullNameAr,
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneCode = Common.PhoneCode,
                PhoneNumber = vm.PhoneNumber,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            };
            try
            {
                IdentityResult result = await _userManager.CreateAsync(user, vm.Password);
                if (!result.Succeeded)
                {
                    result.Errors.Where(x => x.Code == "DuplicateUserName").ToList().
                            ForEach(s => s.Description = "Username is already taken.");
                    return BadRequest(result.Errors.Select(e => e.Description).ToArray());
                }
            }
            catch (Exception ex)
            {
                string error = string.Empty;
                log.Error("Create user api error at identity user create : ");
                log.Error(ex);

                var exceptionType = ex.GetType();
                if (exceptionType == typeof(DbUpdateException))
                {
                    if (ex.InnerException.Message.Contains("NormalizedEmail_UNIQUE"))
                        error = "Email address is already reserved.";
                    else
                        error = ex.InnerException.Message;
                }
                else
                    error = "Failed to create user account. Error" + ex.Message;

                return BadRequest(error);
            }
            #endregion

            #region 2) Create User Role, Table: AspNetUserRole
            IdentityResult result1;
            try
            {
                result1 = await _userManager.AddToRoleAsync(user, vm.RoleName.ToString());
                if (!result1.Succeeded)
                {
                    // delete the user created
                    await _userManager.DeleteAsync(user);
                    return BadRequest(result1.Errors.Select(e => e.Description).ToArray());
                }
            }
            catch (Exception ex)
            {
                log.Error("Create user api error at identity role create : ");
                log.Error(ex);
                // delete the user created
                await _userManager.DeleteAsync(user);
                return BadRequest("Failed to create user account.");
            }
            #endregion

            return Ok(user.Id);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserViewModel vm)
        {
            if (vm.ChangePasswordRequired == true)
            {
                if (string.IsNullOrWhiteSpace(vm.Password))
                {
                    return BadRequest("Valid password is required to update user password.");
                }
            }

            // check if email is already exists
            if (_Db.Users.Any(w => w.Email == vm.Email && w.Id != vm.Id))
                return BadRequest($"Validation Error: User with same email '{vm.Email}' already exists.Create with new email.");
            // check if PhoneNumber is already exists
            //if (_Db.Users.Any(w => w.PhoneNumber == vm.PhoneNumber && w.PhoneCode == Common.PhoneCode && w.Id != vm.Id))
            //    return BadRequest($"Validation Error: User with the same phone code '{vm.PhoneCode}' and phone number '{vm.PhoneNumber}' already exists. Please create with a new phone number or phone code.");


            #region 1) Update User, Table: AspNetUser
            try
            {
                AspNetUser? user = await _userManager.FindByIdAsync(vm.Id.ToString());
                if (user == null)
                    return BadRequest("User not found.");

                #region Update User
                user.FullName = vm.FullName;
                user.FullNameAr = vm.FullNameAr;
                //user.UserName = vm.UserName;
                user.UserName = vm.UserName;
                user.Email = vm.Email;
                user.PhoneCode = Common.PhoneCode;
                user.PhoneNumber = vm.PhoneNumber;
                //user.TwoFactorEnabled = vm.TwoFactorEnabled;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    result.Errors.Where(x => x.Code == "DuplicateUserName").ToList().ForEach(s => s.Description = "Username is already taken.");
                    return BadRequest(result.Errors.Select(e => e.Description).ToArray());
                }
                #endregion

                #region Update Password if required
                if (vm.ChangePasswordRequired == true)
                {
                    var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                    var result_ResetPwd = await _userManager.ResetPasswordAsync(user, token, vm.Password);
                    if (!result_ResetPwd.Succeeded)
                        return BadRequest("Failed to update password.");
                }
                #endregion

                #region Update Role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles[0] != vm.RoleName.ToString())
                {
                    var result_RemoveRole = await _userManager.RemoveFromRoleAsync(user, roles[0]);
                    if (!result_RemoveRole.Succeeded)
                        return BadRequest(result_RemoveRole.Errors.Select(e => e.Description).ToArray());

                    var result_AddRole = await _userManager.AddToRoleAsync(user, vm.RoleName.ToString());
                    if (!result_AddRole.Succeeded)
                        return BadRequest(result_AddRole.Errors.Select(e => e.Description).ToArray());

                }
                #endregion
                return Ok(vm.Id);
            }
            catch (Exception ex)
            {
                log.Error("update user api error at identity user update : ");
                log.Error(ex);
                string error = string.Empty;
                var exceptionType = ex.GetType();
                if (exceptionType == typeof(DbUpdateException))
                {
                    if (ex.InnerException.Message.Contains("NormalizedEmail_UNIQUE"))
                        error = "Email address is already reserved.";
                    else
                        error = ex.InnerException.Message;
                }
                else
                    error = "Failed to update user details.";

                return BadRequest(error);
            }
            #endregion
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                AspNetUser? user = await _Db.Users.FindAsync(id);
                if (user != null)
                {
                    _Db.Remove(user);
                    await _Db.SaveChangesAsync();
                    return Ok(true);
                }
                else
                {
                    return BadRequest(Msg.DeleteRecordNotExists);
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

        [HttpGet("Name")]
        public async Task<IActionResult> GetName()
        {
            try
            {
                bool useArabicName = HttpContext.Request.Headers["Accept-Language"].ToString() == "ar";
                using (var db = _Db)
                {
                    return Ok(await db.Users.Where(w => w.Status != false).Select(s => new
                    {
                        s.Id,
                        s.UserName,
                        Name = useArabicName && !string.IsNullOrEmpty(s.FullNameAr) ? s.FullNameAr : s.FullName,
                    }).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("User2FA/{userId}")]
        public async Task<IActionResult> Update2FA(int userId)
        {
            try
            {
                AspNetUser? user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    user.AccessKey = string.Empty;
                    user.TwoFactorEnabled = !user.TwoFactorEnabled;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    return BadRequest("User data not found.");
                }
                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ActiveInactiveStatus/{userId}")]
        public async Task<IActionResult> ActiveInactiveStatus(int userId)
        {
            try
            {
                AspNetUser? user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    user.AccessKey = string.Empty;
                    user.Status = !user.Status;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    return BadRequest("User data not found.");
                }
                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }



        #region Change Password
        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            AspNetUser? user = await _userManager.FindByIdAsync(vm.Id.ToString());
            if (user == null)
                return BadRequest("User data not found.");

            // ?? Generate a reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // ?? Reset password without old password
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok(true);
        }

        #endregion
    }
}

