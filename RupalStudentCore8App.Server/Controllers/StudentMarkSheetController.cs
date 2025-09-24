using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.ServiceModel;
using RupalStudentCore8App.Server.Services;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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

        //[HttpPost("GetList")]
        //public async Task<IActionResult> GetList(PageModel vm)
        //{
        //    try
        //    {
        //        using (var db = _Db)
        //        {
        //            var dataList = await db.StudentMarkSheets.Where(w => (string.IsNullOrWhiteSpace(vm.SearchText) || w.FormNumber.Contains(vm.SearchText.Trim())
        //                                                  || w.Mobile.Contains(vm.SearchText.Trim()) || w.FamilyName.Contains(vm.SearchText.Trim()) || w.FatherName.Contains(vm.SearchText.Trim()) || w.StudentName.Contains(vm.SearchText.Trim())
        //                                                  ))
        //                .Select(s => new StudentMarkSheet
        //                {
        //                    Id = s.Id,
        //                    FormNumber = s.FormNumber,
        //                    Mobile = s.Mobile,
        //                    FamilyName = s.FamilyName,
        //                    FamilyNameGu = s.FamilyNameGu,
        //                    FatherNameGu = s.FatherNameGu,
        //                    FatherName = s.FatherName,
        //                    StudentName = s.StudentName,
        //                    StudentNameGu = s.StudentNameGu,
        //                    Education = s.Education,
        //                    EducationGu = s.EducationGu,
        //                    SchoolName = s.SchoolName,
        //                    Percentage = s.Percentage,
        //                    Sgpa = s.Sgpa,
        //                    Cgpa = s.Cgpa,
        //                    Status = s.Status
        //                }).ToListAsync();
        //            // Apply sorting in memory using reflection
        //            dataList = ApplySorting(dataList, vm.SortBy, vm.IsAscending);

        //            int totalRecord = dataList.Count();
        //            dataList = dataList.Skip((vm.Page - 1) * vm.PageSize).Take(vm.PageSize).ToList();
        //            return Ok(new { dataList = dataList, totalRecord = totalRecord });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //        return BadRequest("Fail to get data.");
        //    }
        //}

        [HttpPost("GetList")]
        public async Task<IActionResult> GetList(StudentFilterViewModel vm)
        {
            try
            {
                var query = _Db.StudentMarkSheets.Where(w =>
                    (string.IsNullOrWhiteSpace(vm.SearchText)
                     || w.FormNumber.Contains(vm.SearchText.Trim())
                     || w.Mobile.Contains(vm.SearchText.Trim())
                     || w.FamilyName.Contains(vm.SearchText.Trim())
                     || w.FatherName.Contains(vm.SearchText.Trim())
                     || w.StudentName.Contains(vm.SearchText.Trim()))
                    && (vm.Status == null || w.Status == vm.Status)
                );

                int totalRecord = await query.CountAsync();

                // Step 1: get paged data from DB (unsorted)
                var pageData = await query
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .Select(s => new StudentMarkSheet
                    {
                        Id = s.Id,
                        FormNumber = s.FormNumber,
                        Mobile = s.Mobile,
                        FamilyName = s.FamilyName,
                        FamilyNameGu = s.FamilyNameGu,
                        FatherNameGu = s.FatherNameGu,
                        FatherName = s.FatherName,
                        StudentName = s.StudentName,
                        StudentNameGu = s.StudentNameGu,
                        Education = s.Education,
                        EducationGu = s.EducationGu,
                        SchoolName = s.SchoolName,
                        Percentage = s.Percentage,
                        Sgpa = s.Sgpa,
                        Cgpa = s.Cgpa,
                        Status = s.Status,
                        CreatedOn = s.CreatedOn
                    }).OrderByDescending(o => o.FormNumber)
                    .ToListAsync();

                // Step 2: sort in memory
                pageData = ApplySorting(pageData, vm.SortBy, vm.IsAscending);

                return Ok(new { dataList = pageData, totalRecord });
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
                    s.EducationGu,
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

        private List<StudentMarkSheet> ApplySorting(List<StudentMarkSheet> list, string sortBy, bool isAscending)
        {
            try
            {

                if (string.IsNullOrEmpty(sortBy))
                {
                    return list;
                }
                var propertyInfo = typeof(StudentMarkSheet).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);


                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Property '{sortBy}' not found on type '{typeof(StudentMarkSheet)}'");
                }

                if (isAscending)
                {
                    list = list.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }


                return list;
            }
            catch (Exception ex)
            {
                // Log the error to help debug any reflection issues
                log.Error($"Error applying sorting: {ex.Message}", ex);
                return list; // Return unsorted list if an error occurs
            }
        }
    }
}
