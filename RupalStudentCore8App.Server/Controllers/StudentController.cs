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
                StudentMarkSheet entity = await _Db.StudentMarkSheets
                    .FirstOrDefaultAsync(w => w.Id == vm.Id);

                if (entity == null)
                {
                    entity = new StudentMarkSheet
                    {
                        Mobile = vm.Mobile,
                        FamilyName = vm.FamilyName,
                        FamilyNameGu = vm.FamilyNameGu,
                        StudentName = vm.StudentName,
                        StudentNameGu = vm.StudentNameGu,
                        FatherName = vm.FatherName,
                        FatherNameGu = vm.FatherNameGu,
                        Education = vm.Education,
                        EducationGu = vm.EducationGu,
                        SchoolName = vm.SchoolName,
                        Percentage = vm.Percentage,
                        Sgpa = vm.Sgpa,
                        Cgpa = vm.Cgpa,
                        AcademicYear = DateTime.Now.Year.ToString(),
                        Status = StudentStatus.New,
                        FormNumber = _IUtility.AutoIncrement(GlobalConstant.AutoIncrement.Student, true),
                        CreatedOn = DateTime.Now
                    };
                    _Db.StudentMarkSheets.Add(entity);
                    await _Db.SaveChangesAsync();
                }
                else
                {
                    entity.Mobile = vm.Mobile;
                    entity.FamilyName = vm.FamilyName;
                    entity.FamilyNameGu = vm.FamilyNameGu;
                    entity.StudentName = vm.StudentName;
                    entity.StudentNameGu = vm.StudentNameGu;
                    entity.FatherName = vm.FatherName;
                    entity.FatherNameGu = vm.FatherNameGu;
                    entity.Education = vm.Education;
                    entity.EducationGu = vm.EducationGu;
                    entity.SchoolName = vm.SchoolName;
                    entity.Percentage = vm.Percentage;
                    entity.Sgpa = vm.Sgpa;
                    entity.Cgpa = vm.Cgpa;
                    entity.AcademicYear = DateTime.Now.Year.ToString();
                    entity.Status = vm.IsApproved ? StudentStatus.Approved : StudentStatus.New;
                    _Db.StudentMarkSheets.Update(entity);
                    await _Db.SaveChangesAsync();
                }

                // Handle attachments
                if (vm.Attachments?.Any() == true)
                {
                    var oldAttachment = await _Db.Attachments
                        .Where(w => w.ReferenceId == entity.Id && w.ReferenceType == AttachmentReferenceType.Student)
                        .ToListAsync();

                    _Db.Attachments.RemoveRange(oldAttachment);

                    List<Attachment> newAttachments = new();
                    foreach (var item in vm.Attachments)
                    {
                        string fileName = item.File != null
                            ? _IUtility.UploadFile(item.File, FilePath.Student) // returns relative filename
                            : (!string.IsNullOrEmpty(item.FileUrl) ? Path.GetFileName(item.FileUrl) : null);

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            newAttachments.Add(new Attachment
                            {
                                ReferenceId = entity.Id,
                                ReferenceType = item.ReferenceType,
                                FileName = item.File?.FileName ?? item.FileName,
                                Description = item.Description,
                                FileUrl = fileName // ✅ only relative filename stored in DB
                            });
                        }
                    }

                    if (newAttachments.Any())
                    {
                        await _Db.Attachments.AddRangeAsync(newAttachments);
                        await _Db.SaveChangesAsync();
                    }
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



        //[HttpPost]
        //public async Task<IActionResult> AddEdit([FromForm] StudentViewModel vm)
        //{
        //    using var transaction = await _Db.Database.BeginTransactionAsync();
        //    try
        //    {
        //        StudentMarkSheet entity;
        //        entity = await _Db.StudentMarkSheets.Where(w => w.Id == vm.Id).FirstOrDefaultAsync();

        //        if (entity == null)
        //        {
        //            entity = new StudentMarkSheet();
        //            entity.Mobile = vm.Mobile;
        //            entity.FamilyName = vm.FamilyName;
        //            entity.FamilyNameGu = vm.FamilyNameGu;
        //            entity.StudentName = vm.StudentName;
        //            entity.StudentNameGu = vm.StudentNameGu;
        //            entity.FatherName = vm.FatherName;
        //            entity.FatherNameGu = vm.FatherNameGu;
        //            entity.Education = vm.Education;
        //            entity.EducationGu = vm.EducationGu;
        //            entity.SchoolName = vm.SchoolName;
        //            entity.Percentage = vm.Percentage;
        //            entity.Sgpa = vm.Sgpa;
        //            entity.Cgpa = vm.Cgpa;
        //            entity.AcademicYear = DateTime.Now.Year.ToString();
        //            entity.Status = StudentStatus.New;
        //            entity.FormNumber = _IUtility.AutoIncrement(GlobalConstant.AutoIncrement.Student, true);
        //            entity.CreatedOn = DateTime.Now;
        //            _Db.StudentMarkSheets.Add(entity);
        //            await _Db.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            entity.Mobile = vm.Mobile;
        //            entity.FamilyName = vm.FamilyName;
        //            entity.FamilyNameGu = vm.FamilyNameGu;
        //            entity.StudentName = vm.StudentName;
        //            entity.StudentNameGu = vm.StudentNameGu;
        //            entity.FatherName = vm.FatherName;
        //            entity.FatherNameGu = vm.FatherNameGu;
        //            entity.Education = vm.Education;
        //            entity.EducationGu = vm.EducationGu;
        //            entity.SchoolName = vm.SchoolName;
        //            entity.Percentage = vm.Percentage;
        //            entity.Sgpa = vm.Sgpa;
        //            entity.Cgpa = vm.Cgpa;
        //            entity.AcademicYear = DateTime.Now.Year.ToString();
        //            entity.Status = vm.IsApproved ? StudentStatus.Approved : StudentStatus.New;
        //            _Db.StudentMarkSheets.Update(entity);
        //            await _Db.SaveChangesAsync();
        //        }
        //        if (vm.Attachments?.Any() == true)
        //        {
        //            var oldAttachment = _Db.Attachments.Where(w => w.ReferenceId == entity.Id && w.ReferenceType == AttachmentReferenceType.Student).ToList();
        //            _Db.Attachments.RemoveRange(oldAttachment);

        //            List<Attachment> newAttachments = new();
        //            foreach (var item in vm.Attachments)
        //            {
        //                string fileName = item.File != null
        //                    ? _IUtility.UploadFile(item.File, FilePath.Student)
        //                    : (!string.IsNullOrEmpty(item.FileUrl) ? Path.GetFileName(item.FileUrl) : null);

        //                newAttachments.Add(new Attachment
        //                {
        //                    ReferenceId = entity.Id,
        //                    ReferenceType = item.ReferenceType,
        //                    FileName = item.File.FileName,
        //                    Description = item.Description,
        //                    FileUrl = fileName
        //                });
        //            }
        //            await _Db.Attachments.AddRangeAsync(newAttachments);
        //            await _Db.SaveChangesAsync();
        //        }
        //        await transaction.CommitAsync();
        //        return Ok(entity.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> AttachmentDelete(int id)
        {
            try
            {
                using (var db = _Db)
                {
                    Attachment entity = await db.Attachments.FindAsync(id);
                    if (entity != null)
                    {
                        db.Remove(entity);
                        await db.SaveChangesAsync();
                        return Ok(true);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to delete data.");
            }
        }

   

    }
}
