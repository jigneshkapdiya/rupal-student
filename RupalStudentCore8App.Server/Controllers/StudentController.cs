using RupalStudentCore8App.Server.Class;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.ServiceModel;
using RupalStudentCore8App.Server.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;
        private readonly IUtility _IUtility;

        public StudentController(ApplicationDbContext Db, IUtility utility)
        {
            _Db = Db;
            _IUtility = utility;
        }

        [HttpGet("StudentEducation/Name")]
        public async Task<IActionResult> StudentEductionNameList()
        {
            try
            {
                using (var db = _Db)
                {
                    return Ok(await db.StudentEducations.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.NameGu
                    }).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit([FromForm] StudentViewModel vm)
        {
            using var transaction = await _Db.Database.BeginTransactionAsync();
            try
            {
                // 1. Check if student already exists (for Edit case)
                StudentMarkSheet entity;

                // 2. Map data from ViewModel to Entity
                entity = new StudentMarkSheet();
                entity.Mobile = vm.Mobile;
                entity.FamilyName = vm.FamilyName;
                entity.FamilyNameGu = vm.FamilyNameGu;
                entity.StudentName = vm.StudentName;
                entity.StudentNameGu = vm.StudentNameGu;
                entity.FatherName = vm.FatherName;
                entity.FatherNameGu = vm.FatherNameGu;
                entity.Education = vm.Education;
                entity.SchoolName = vm.SchoolName;
                entity.Percentage = vm.Percentage;
                entity.Sgpa = vm.Sgpa;
                entity.Cgpa = vm.Cgpa;
                entity.AcademicYear = DateTime.Now.Year.ToString();
                entity.Status ="New";
                entity.FormNumber = _IUtility.AutoIncrement(GlobalConstant.AutoIncrement.Student, true);
                _Db.StudentMarkSheets.Add(entity);
                await _Db.SaveChangesAsync();

                // 3. Handle Attachments
                if (vm.Attachments?.Any() == true)
                {
                    List<Attachment> newAttachments = new();

                    foreach (var item in vm.Attachments)
                    {
                        string fileName = item.File != null
                            ? _IUtility.UploadFile(item.File, FilePath.Student)
                            : (!string.IsNullOrEmpty(item.FileUrl) ? Path.GetFileName(item.FileUrl) : null);

                        newAttachments.Add(new Attachment
                        {
                            ReferenceId = entity.Id,
                            ReferenceType = item.ReferenceType,
                            FileName = item.File.FileName,
                            Description = item.Description,
                            FileUrl = fileName
                        });
                    }

                    await _Db.Attachments.AddRangeAsync(newAttachments);
                    await _Db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
