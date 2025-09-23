using ClosedXML.Excel;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RupalStudentCore8App.Server.Class;
using RupalStudentCore8App.Server.Data;
using RupalStudentCore8App.Server.Entities;
using RupalStudentCore8App.Server.ServiceModel;
using RupalStudentCore8App.Server.Services;
using System.Drawing;
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

        [HttpDelete("Student/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var attachmentList = await _Db.Attachments.Where(w => w.ReferenceId == id).ToListAsync();
                _Db.Attachments.RemoveRange(attachmentList);

                StudentMarkSheet studentMarkSheet = await _Db.StudentMarkSheets.FindAsync(id);
                if (studentMarkSheet != null)
                {
                    _Db.StudentMarkSheets.Remove(studentMarkSheet);
                    await _Db.SaveChangesAsync();
                    return Ok(true);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to delete data.");
            }
        }

        [HttpPost("ExportStudentList")]
        public async Task<IActionResult> ExportToExcel(StudentFilterViewModel vm)
        {
            try
            {
                var query = _Db.StudentMarkSheets.Where(w => (string.IsNullOrWhiteSpace(vm.SearchText) || w.FormNumber.Contains(vm.SearchText.Trim()) || w.Mobile.Contains(vm.SearchText.Trim()) || w.FamilyName.Contains(vm.SearchText.Trim())
                                || w.FatherName.Contains(vm.SearchText.Trim()) || w.StudentName.Contains(vm.SearchText.Trim())) && (vm.Status == null || w.Status == vm.Status));

                int totalRecord = await query.CountAsync();

                // Step 1: get paged data from DB (unsorted)
                var pageData = await query.Skip((vm.Page - 1) * vm.PageSize).Take(vm.PageSize).Select(s => new StudentMarkSheet
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
                }).OrderByDescending(o => o.FormNumber).ToListAsync();

                // Step 2: sort in memory
                pageData = ApplySorting(pageData, vm.SortBy, vm.IsAscending);

                byte[] finalResult;
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Student");
                    var currentRow = 1;

                    worksheet.Row(currentRow).Style.Font.Bold = true;

                    // Headers
                    worksheet.Cell(currentRow, 1).Value = "Date";
                    worksheet.Cell(currentRow, 2).Value = "NO.";
                    worksheet.Cell(currentRow, 3).Value = "Student Name";
                    worksheet.Cell(currentRow, 4).Value = "Father Name";
                    worksheet.Cell(currentRow, 5).Value = "Mobile";
                    worksheet.Cell(currentRow, 6).Value = "Family Name";
                    worksheet.Cell(currentRow, 7).Value = "Education";
                    worksheet.Cell(currentRow, 8).Value = "Percentage";
                    worksheet.Cell(currentRow, 9).Value = "CGPA";
                    worksheet.Cell(currentRow, 10).Value = "SGPA";
                    worksheet.Cell(currentRow, 11).Value = "Status"; // Fixed duplicate column index

                    for (int i = 0; i < pageData.Count; i++)
                    {
                        currentRow++;

                        worksheet.Cell(currentRow, 1).Value = pageData[i].CreatedOn;
                        worksheet.Cell(currentRow, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                        worksheet.Cell(currentRow, 2).Value = pageData[i].FormNumber?.ToString(); // Use ToString() for text format
                        worksheet.Cell(currentRow, 3).Value = pageData[i].StudentName;
                        worksheet.Cell(currentRow, 4).Value = pageData[i].FatherName; // Fixed column index
                        worksheet.Cell(currentRow, 5).Value = pageData[i].Mobile;
                        worksheet.Cell(currentRow, 6).Value = pageData[i].FamilyName;
                        worksheet.Cell(currentRow, 7).Value = pageData[i].Education;
                        worksheet.Cell(currentRow, 8).Value = pageData[i].Percentage;
                        worksheet.Cell(currentRow, 9).Value = pageData[i].Cgpa;
                        worksheet.Cell(currentRow, 10).Value = pageData[i].Sgpa;
                        worksheet.Cell(currentRow, 11).Value = pageData[i].Status; // Fixed column index

                        // If you need to set specific format for certain columns, use Style.NumberFormat
                        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "@"; // Text format for FormNumber
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "@"; // Text format for Mobile

                        // Format percentage column
                        if (pageData[i].Percentage.HasValue)
                        {
                            worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "0.00";
                        }

                        // Format CGPA/SGPA columns
                        if (pageData[i].Cgpa.HasValue)
                        {
                            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "0.00";
                        }

                        if (pageData[i].Sgpa.HasValue)
                        {
                            worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "0.00";
                        }
                    }

                    // Auto-fit columns for better visibility
                    worksheet.Columns().AdjustToContents();

                    // Setting borders to each used cell in excel
                    var dataRange = worksheet.Range(worksheet.Cell(1, 1), worksheet.Cell(currentRow, 11));
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.BottomBorderColor = XLColor.Black;
                    dataRange.Style.Border.TopBorderColor = XLColor.Black;
                    dataRange.Style.Border.LeftBorderColor = XLColor.Black;
                    dataRange.Style.Border.RightBorderColor = XLColor.Black;

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    finalResult = stream.ToArray();
                }
                return File(finalResult, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Student.xlsx");
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
