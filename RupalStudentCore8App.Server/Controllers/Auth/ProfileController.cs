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
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger((MethodBase.GetCurrentMethod()?.DeclaringType) ?? typeof(ProfileController));
        private readonly ApplicationDbContext _Db;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUtility _IUtility;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProfileController(UserManager<AspNetUser> userManager, SignInManager<AspNetUser> signInManager, ApplicationDbContext Db, IConfiguration configuration, IUtility utility, IWebHostEnvironment hostingEnvironment)
        {
            _Db = Db;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _IUtility = utility;
            _hostingEnvironment = hostingEnvironment;
        }

        #region GetUser
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
                if (userId == 0)
                    return BadRequest("User data not found");

                using (var db = _Db)
                {
                    return Ok(await db.Users.Where(w => w.Id == userId).Select(s => new
                    {
                        s.Id,
                        s.UserName,
                        s.FullName,
                        s.FullNameAr,
                        s.Email,
                        s.PhoneNumber,
                        s.Status,
                        s.TwoFactorEnabled,
                        ProfileImage = _IUtility.GetFileUrl(s.ProfileImage, FileType.ProfileImage),
                        Allow2FA = _configuration["Allow2FA"],
                    }).FirstOrDefaultAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Edit Profile
        [HttpPut]
        public async Task<IActionResult> EditProfile([FromForm] ProfileViewModel vm)
        {
            try
            {
                var users = await _userManager.FindByIdAsync(vm.Id.ToString());
                if (users == null)
                    return BadRequest("User data not found.");

                users.FullName = vm.FullName;
                users.FullNameAr = vm.FullNameAr;
                users.UserName = vm.UserName;
                users.Email = vm.Email;
                users.PhoneNumber = vm.PhoneNumber;

                if (vm.ProfileImage != null)
                {
                    users.ProfileImage = _IUtility.UploadFile(vm.ProfileImage, FilePath.ProfileImage);
                }
                var result = await _userManager.UpdateAsync(users);

                if (result.Succeeded)
                {
                    return Ok(vm);
                }
                else
                {
                    result.Errors.Where(x => x.Code == "DuplicateUserName").ToList().ForEach(s => s.Description = "Username is already taken.");
                    return BadRequest(result.Errors.Select(e => e.Description).ToArray());
                }

            }
            catch (Exception ex)
            {
                log.Error("update user api error at identity user update : ");
                log.Error(ex);
                string message = string.Empty;
                var exceptionType = ex.GetType();
                if (exceptionType == typeof(Microsoft.EntityFrameworkCore.DbUpdateException))
                {
                    if (ex.InnerException.Message.Contains("NormalizedEmail_UNIQUE"))
                        message = "Email address is already reserved";
                    else
                        message = ex.InnerException.Message;
                }
                else
                    message = "Failed to update user";

                return BadRequest(message);

            }
        }
        #endregion

        #region Change Password
        [Route("ChangePassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest vm)
        {
            int userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
            if (userId == 0)
                return BadRequest("User data not found");

            AspNetUser? user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return BadRequest("User data not found.");

            var result = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);

            if (!result.Succeeded)
                return BadRequest("Incorrect password.");


            return Ok(true);
        }
        #endregion

        //[Route("GenerateBarcode")]
        //[HttpGet]
        //public async Task<IActionResult> GenerateBarcode()
        //{
        //    try
        //    {
        //        int userId = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value);
        //        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
        //        string AccessKey = GetUniqueKey(10) + userId;
        //        var SetupInfo = tfa.GenerateSetupCode("Excomatix", "Apex", AccessKey, false, 3);
        //        AspNetUser user = await _userManager.FindByIdAsync(userId.ToString());
        //        user.AccessKey = AccessKey;
        //        await _userManager.UpdateAsync(user);
        //        return Ok(new { barcodeImg = SetupInfo.QrCodeSetupImageUrl, manualCode = SetupInfo.ManualEntryKey });
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("VerificationCode")]
        //public async Task<IActionResult> VerifyCode(string VerificationCode)
        //{
        //    try
        //    {
        //        if (VerificationCode == null)
        //        {
        //            return BadRequest("VerificationCode is required");
        //        }
        //        int userId = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value);
        //        AspNetUser user = await _userManager.FindByIdAsync(userId.ToString());
        //        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
        //        bool d = tfa.ValidateTwoFactorPIN(user.AccessKey, VerificationCode);
        //        if (d)
        //        {
        //            user.TwoFactorEnabled = true;
        //            await _userManager.UpdateAsync(user);
        //            return Ok(new { message = "Success" });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Invalid Verification Code." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[Route("DisableTFA")]
        //[HttpPost]
        //public async Task<IActionResult> DesableTFA(string Password)
        //{
        //    try
        //    {
        //        int userId = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value);
        //        User user = await _userManager.FindByIdAsync(userId.ToString());
        //        var validatePassword = await _signInManager.CheckPasswordSignInAsync(user, Password, false);
        //        if (!validatePassword.Succeeded)
        //        {
        //            return BadRequest(new { message = "Invalid password" });
        //        }
        //        user.TwoFactorEnabled = false;
        //        await _userManager.UpdateAsync(user);
        //        return Ok(new { message = "Success" });
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        //private static string GetUniqueKey(int maxSize)
        //{
        //    char[] chars = new char[62];
        //    chars =
        //    "zyxabcwvudeftsrghiqpojklnmABYZCDWXEFUVEFSTGHQRIJOPKLMN7861239045".ToCharArray();
        //    byte[] data = new byte[1];
        //    using (RNGCryptoServiceProvider crypto = new())
        //    {
        //        crypto.GetNonZeroBytes(data);
        //        data = new byte[maxSize];
        //        crypto.GetNonZeroBytes(data);
        //    }
        //    StringBuilder result = new StringBuilder(maxSize);
        //    foreach (byte b in data)
        //    {
        //        result.Append(chars[b % (chars.Length)]);
        //    }
        //    return Convert.ToString(result);
        //}

        #region 2FA
        [HttpGet("DisableTFA/{userId}")]
        public async Task<IActionResult> Update2FA(int userId)
        {
            try
            {
                AspNetUser? user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
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
        #endregion
    }
}
