using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Models.Auth;
using RupalStudentCore8App.Server.Services;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StudentMarkSheetController : ControllerBase
    {
        #region Init
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;
        private readonly IUtility _utility;
        public StudentMarkSheetController(ApplicationDbContext Db, IUtility utility)
        {
            _Db = Db;
            _utility = utility;
        }
        #endregion

        [HttpPost("GetList")]
        public async Task<IActionResult> GetList(PageModel vm)
        {
            try
            {
                using (var db = _Db)
                {
                    var dataList = await db.StudentMarkSheets.Where(w => (string.IsNullOrWhiteSpace(vm.SearchText) || w.FormNumber.Contains(vm.SearchText.Trim())
                                                          || w.Mobile.Contains(vm.SearchText.Trim()) || w.FamilyName.Contains(vm.SearchText.Trim()) || w.FatherName.Contains(vm.SearchText.Trim()) || w.StudentName.Contains(vm.SearchText.Trim())
                                                          ))
                        .Select(s => new
                        {
                            s.Id,
                            s.FormNumber,
                            s.Mobile,
                            s.FamilyName,
                            s.FamilyNameGu,
                            s.FatherNameGu,
                            s.FatherName,
                            s.StudentName,
                            s.StudentNameGu,
                            s.Education,
                            s.SchoolName,
                            s.Percentage,
                            s.Sgpa,
                            s.Cgpa,
                            s.Status
                        }).ToListAsync();
                    int totalRecord = dataList.Count();
                    dataList = dataList.Skip((vm.Page - 1) * vm.PageSize).Take(vm.PageSize).ToList();
                    return Ok(new { dataList = dataList, totalRecord = totalRecord });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var list = await _Db.StudentMarkSheets.Where(w => w.Id == id).Select(s => new
                {
                    s.Id,
                    s.FormNumber,
                    s.Mobile,
                    s.FamilyName,
                    s.FamilyNameGu,
                    s.StudentName,
                    s.StudentNameGu,
                    s.FatherName,
                    s.FatherNameGu,
                    s.Education,
                    s.SchoolName,
                    s.Percentage,
                    s.Sgpa,
                    s.Cgpa,
                    s.AcademicYear,
                    s.Status,
                    AttachmentList = _Db.Attachments.Where(w => w.ReferenceId == s.Id && w.ReferenceType == AttachmentReferenceType.Student).Select(s => new
                    {
                        s.Id,
                        s.ReferenceId,
                        s.ReferenceType,
                        s.FileName,
                        FileUrl = _utility.GetFileUrl(s.FileUrl, s.ReferenceType),
                        s.Description,
                    }).ToList()
                }).FirstOrDefaultAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
            }
        }
    }
}
