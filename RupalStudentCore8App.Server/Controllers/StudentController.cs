using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RupalStudentCore8App.Server.Class;
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

        [HttpPost]
        public async Task<IActionResult> AddEdit([FromForm] StudentViewModel vm)
        {
            using var transaction = await _Db.Database.BeginTransactionAsync();
            try
            {
                StudentMarkSheet entity = await _Db.StudentMarkSheets.FirstOrDefaultAsync(w => w.Id == vm.Id);
                // Get max sequence as integer for the same education
                int maxGroupSeq = (int)(_Db.StudentMarkSheets
           .Where(s => s.Education == vm.Education)
           .AsEnumerable()
           .Select(s => s.GroupSequenceNumber)
           .DefaultIfEmpty(0)
           .Max() ?? 0);

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
                        CreatedOn = DateTime.Now,
                        Semester = vm.Semester,
                        SequenceNumber = vm.SequenceNumber,
                        Grade = vm.Grade,
                        Description = vm.Description
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
                    //entity.Status = vm.IsApproved ? StudentStatus.Approved : (vm.IsRejected ? StudentStatus.Rejected : StudentStatus.New);
                    entity.Semester = vm.Semester;
                    entity.SequenceNumber = vm.SequenceNumber;
                    entity.Status = vm.Status;
                    entity.Grade = vm.Grade;
                    entity.Description = vm.Description;
                    if (vm.SequenceNumber != 0)
                    {
                        entity.GroupSequenceNumber = (maxGroupSeq + 1);
                    }
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
                            : (string.IsNullOrEmpty(item.FileUrl) ? null : System.IO.Path.GetFileName(item.FileUrl));

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

        [HttpGet("GetStudentList")]
        public async Task<IActionResult> GetStudentList()
        {
            try
            {
                var list = await _Db.StudentMarkSheets.Select(s => new
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
                    s.CreatedOn,
                    s.Semester,
                    s.Grade,
                    s.Description
                }).OrderByDescending(o => o.FormNumber).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
            }
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
                    s.Semester,
                    s.SequenceNumber,
                    s.Grade,
                    s.Description,
                    AttachmentList = _Db.Attachments.Where(w => w.ReferenceId == s.Id && w.ReferenceType == AttachmentReferenceType.Student).Select(s => new
                    {
                        s.Id,
                        s.ReferenceId,
                        s.ReferenceType,
                        s.FileName,
                        FileUrl = _IUtility.GetFileUrl(s.FileUrl, s.ReferenceType),
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

                // Get education order from StudentEducation table
                var educationOrder = await _Db.StudentEducations
                    .ToDictionaryAsync(e => e.Name, e => e.SequenceNo);

                // Simple grouping logic - always apply education order first
                var allRecords = await query
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
                        CreatedOn = s.CreatedOn,
                        Semester = s.Semester,
                        Grade = s.Grade,
                        Description = s.Description
                    })
                    .ToListAsync();

                // Apply education order sorting first, then other sorting
                var sortedRecords = allRecords
                    .OrderBy(r => educationOrder.ContainsKey(r.Education) ? educationOrder[r.Education] : int.MaxValue)
                    .ThenBy(r => r.Education)
                    .ToList();

                // Apply your existing sorting
                sortedRecords = ApplySorting(sortedRecords, vm.SortBy, vm.IsAscending);

                // Pagination
                var pageData = sortedRecords
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                return Ok(new { dataList = pageData, totalRecord });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
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
                var query = _Db.StudentMarkSheets.Where(w =>
                    (string.IsNullOrWhiteSpace(vm.SearchText) ||
                     w.FormNumber.Contains(vm.SearchText.Trim()) ||
                     w.Mobile.Contains(vm.SearchText.Trim()) ||
                     w.FamilyName.Contains(vm.SearchText.Trim()) ||
                     w.FatherName.Contains(vm.SearchText.Trim()) ||
                     w.StudentName.Contains(vm.SearchText.Trim())) &&
                    (vm.Status == null || w.Status == vm.Status));

                int totalRecord = await query.CountAsync();

                // Step 1: get paged data
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
                    })
                    .OrderByDescending(o => o.FormNumber)
                    .ToListAsync();

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
                    worksheet.Cell(currentRow, 2).Value = "Form Number";
                    worksheet.Cell(currentRow, 3).Value = "Student Name";
                    worksheet.Cell(currentRow, 4).Value = "Father Name";
                    worksheet.Cell(currentRow, 5).Value = "Mobile";
                    worksheet.Cell(currentRow, 6).Value = "Family Name";
                    worksheet.Cell(currentRow, 7).Value = "Education";
                    worksheet.Cell(currentRow, 8).Value = "Percentage";
                    worksheet.Cell(currentRow, 9).Value = "CGPA";
                    worksheet.Cell(currentRow, 10).Value = "SGPA";
                    worksheet.Cell(currentRow, 11).Value = "Status";

                    // Data rows
                    for (int i = 0; i < pageData.Count; i++)
                    {
                        currentRow++;

                        worksheet.Cell(currentRow, 1).Value = pageData[i].CreatedOn;
                        worksheet.Cell(currentRow, 1).Style.DateFormat.Format = "dd/MM/yyyy";

                        worksheet.Cell(currentRow, 2).Value = pageData[i].FormNumber?.ToString();

                        // Student Name (Gujarati + English in same cell)
                        worksheet.Cell(currentRow, 3).Value = pageData[i].StudentNameGu + Environment.NewLine + pageData[i].StudentName;
                        worksheet.Cell(currentRow, 3).Style.Alignment.WrapText = true;

                        // Father Name
                        worksheet.Cell(currentRow, 4).Value = pageData[i].FatherNameGu + Environment.NewLine + pageData[i].FatherName;
                        worksheet.Cell(currentRow, 4).Style.Alignment.WrapText = true;

                        // Mobile
                        worksheet.Cell(currentRow, 5).Value = pageData[i].Mobile;

                        // Family Name
                        worksheet.Cell(currentRow, 6).Value = pageData[i].FamilyNameGu + Environment.NewLine + pageData[i].FamilyName;
                        worksheet.Cell(currentRow, 6).Style.Alignment.WrapText = true;

                        // Education
                        worksheet.Cell(currentRow, 7).Value = pageData[i].EducationGu + Environment.NewLine + pageData[i].Education;
                        worksheet.Cell(currentRow, 7).Style.Alignment.WrapText = true;

                        // Other fields
                        worksheet.Cell(currentRow, 8).Value = pageData[i].Percentage;
                        worksheet.Cell(currentRow, 9).Value = pageData[i].Cgpa;
                        worksheet.Cell(currentRow, 10).Value = pageData[i].Sgpa;
                        worksheet.Cell(currentRow, 11).Value = pageData[i].Status;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Borders
                    var dataRange = worksheet.Range(worksheet.Cell(1, 1), worksheet.Cell(currentRow, 12));
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

        [HttpPost("ExportPDF")]
        public async Task<IActionResult> GetStudentExportPdf(StudentFilterViewModel vm)
        {
            try
            {
                // Get filtered student data
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

                // Get paged data from DB
                var students = await query
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
                    })
                    .OrderByDescending(o => o.FormNumber)
                    .ToListAsync();

                // Sort in memory
                students = ApplySorting(students, vm.SortBy, vm.IsAscending);

                if (students == null || !students.Any())
                {
                    return BadRequest("No student data found.");
                }

                // Create PDF document
                using (MemoryStream ms = new MemoryStream())
                {
                    var writer = new PdfWriter(ms);
                    var pdfDoc = new PdfDocument(writer);
                    var document = new Document(pdfDoc, PageSize.A4.Rotate());

                    // Add title
                    document.Add(new Paragraph("Students MarkSheet Reports")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

                    document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.GRAY));

                    document.Add(new Paragraph(" ")); // Empty line

                    // Create table with 11 columns
                    Table table = new Table(11, false);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    // Add table headers
                    string[] headers = { "Date", "Form Number", "Student Name", "Father Name", "Mobile", "Family Name", "Education", "Percentage", "CGPA", "SGPA", "Status" };

                    foreach (var header in headers)
                    {
                        table.AddHeaderCell(new Cell()
                            .Add(new Paragraph(header))
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(TextAlignment.CENTER));
                    }

                    // Add student data rows with dual language support
                    foreach (var student in students)
                    {
                        // Date cell
                        table.AddCell(CreateDualLanguageCell(student.CreatedOn?.ToString("dd-MM-yyyy") ?? "", ""));

                        // Form Number cell
                        table.AddCell(CreateDualLanguageCell(student.FormNumber ?? "", ""));

                        // Student Name cell - Gujarati above English
                        table.AddCell(CreateDualLanguageCell(student.StudentNameGu ?? "", student.StudentName ?? ""));

                        // Father Name cell - Gujarati above English
                        table.AddCell(CreateDualLanguageCell(student.FatherNameGu ?? "", student.FatherName ?? ""));

                        // Mobile cell
                        table.AddCell(CreateDualLanguageCell(student.Mobile ?? "", ""));

                        // Family Name cell - Gujarati above English
                        table.AddCell(CreateDualLanguageCell(student.FamilyNameGu ?? "", student.FamilyName ?? ""));

                        // Education cell - Gujarati above English
                        table.AddCell(CreateDualLanguageCell(student.EducationGu ?? "", student.Education ?? ""));

                        // Percentage cell
                        table.AddCell(new Cell()
                            .Add(new Paragraph(student.Percentage?.ToString() ?? ""))
                            .SetTextAlignment(TextAlignment.RIGHT));

                        // CGPA cell
                        table.AddCell(new Cell()
                            .Add(new Paragraph(student.Cgpa?.ToString() ?? ""))
                            .SetTextAlignment(TextAlignment.RIGHT));

                        // SGPA cell
                        table.AddCell(new Cell()
                            .Add(new Paragraph(student.Sgpa?.ToString() ?? ""))
                            .SetTextAlignment(TextAlignment.RIGHT));

                        // Status cell
                        table.AddCell(CreateDualLanguageCell(student.Status ?? "", ""));
                    }

                    document.Add(table);

                    // Add summary/footer
                    document.Add(new Paragraph($"Total Records: {students.Count}")
                        .SetFontSize(10)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                        .SetTextAlignment(TextAlignment.RIGHT));

                    document.Close();

                    byte[] pdfBytes = ms.ToArray();
                    ms.Close();
                    return File(pdfBytes, "application/pdf", $"Students_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return BadRequest("Fail to download data.");
            }
        }

        // Helper method to create cells with dual language text (Gujarati above English)
        private Cell CreateDualLanguageCell(string gujaratiText, string englishText)
        {
            var cell = new Cell();

            // Combine Gujarati and English in one paragraph vertically
            if (!string.IsNullOrEmpty(gujaratiText))
            {
                var guText = new Paragraph(gujaratiText)
                    .SetFontSize(10)
                    .SetBold() // Gujarati can be bold
                    .SetTextAlignment(TextAlignment.LEFT);
                cell.Add(guText);
            }

            if (!string.IsNullOrEmpty(englishText))
            {
                var enText = new Paragraph(englishText)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.DARK_GRAY)
                    .SetTextAlignment(TextAlignment.LEFT);
                cell.Add(enText);
            }

            // If both are empty, add empty paragraph
            if (string.IsNullOrEmpty(gujaratiText) && string.IsNullOrEmpty(englishText))
            {
                cell.Add(new Paragraph(""));
            }

            // Optional: add padding to make it look neat
            cell.SetPadding(5);

            return cell;
        }

        [HttpPost("GetList2")]
        public async Task<IActionResult> GetList2(StudentFilterViewModel vm)
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

                // ✅ Get education order dictionary
                //var educationOrder = await _Db.StudentEducations
                //    .OrderBy(e => e.Id) // Assuming Id = sequence order
                //    .ToDictionaryAsync(e => e.Name, e => e.Id);
                var educationOrder = await _Db.StudentEducations.Select(e => new { e.Name, e.SequenceNo }).ToDictionaryAsync(e => e.Name, e => e.SequenceNo);

                // ✅ Pull all records first
                var allRecords = await query
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
                        s.EducationGu,
                        s.SchoolName,
                        s.Percentage,
                        s.Sgpa,
                        s.Cgpa,
                        s.Status,
                        s.CreatedOn,
                        s.Semester,
                        s.Grade,
                        s.Description
                    })
                    .ToListAsync();

                // ✅ Final ranked list
                var rankedList = new List<object>();

                // Group by Education in correct sequence
                var grouped = allRecords
                    .GroupBy(r => r.Education)
                    .OrderBy(g => educationOrder.ContainsKey(g.Key) ? educationOrder[g.Key] : int.MaxValue);

                foreach (var group in grouped)
                {
                    // Sort inside education group
                    var ordered = group.OrderByDescending(x => x.Percentage ?? 0)
                                       .ThenByDescending(x => x.Sgpa ?? 0)
                                       .ThenByDescending(x => x.Cgpa ?? 0)
                                       .ToList();

                    int rank = 1;
                    foreach (var s in ordered)
                    {
                        rankedList.Add(new
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
                            s.EducationGu,
                            s.SchoolName,
                            s.Percentage,
                            s.Sgpa,
                            s.Cgpa,
                            s.Status,
                            s.CreatedOn,
                            s.Semester,
                            s.Grade,
                            s.Description,
                            Rank = rank++
                        });
                    }
                }

                // ✅ Pagination
                var pageData = rankedList
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                return Ok(new { dataList = pageData, totalRecord });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
            }
        }

        [HttpPost("ExportStudentList2")]
        public async Task<IActionResult> ExportToExcel2(StudentFilterViewModel vm)
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

                // ✅ Get education order dictionary
                var educationOrder = await _Db.StudentEducations
                    .OrderBy(e => e.Id) // Assuming Id = sequence order
                    .ToDictionaryAsync(e => e.Name, e => e.Id);

                // ✅ Pull all records first
                var allRecords = await query
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
                        s.EducationGu,
                        s.SchoolName,
                        s.Percentage,
                        s.Sgpa,
                        s.Cgpa,
                        s.Status,
                        s.CreatedOn,
                        s.Semester,
                        s.Grade,
                        s.Description
                    })
                    .ToListAsync();

                // ✅ Final ranked list
                var rankedList = new List<dynamic>();

                // Group by Education in correct sequence
                var grouped = allRecords
                    .GroupBy(r => r.Education)
                    .OrderBy(g => educationOrder.ContainsKey(g.Key) ? educationOrder[g.Key] : int.MaxValue);

                foreach (var group in grouped)
                {
                    // Sort inside education group
                    var ordered = group.OrderByDescending(x => x.Percentage ?? 0)
                                       .ThenByDescending(x => x.Sgpa ?? 0)
                                       .ThenByDescending(x => x.Cgpa ?? 0)
                                       .ToList();

                    int rank = 1;
                    foreach (var s in ordered)
                    {
                        rankedList.Add(new
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
                            s.EducationGu,
                            s.SchoolName,
                            s.Percentage,
                            s.Sgpa,
                            s.Cgpa,
                            s.Status,
                            s.CreatedOn,
                            s.Semester,
                            s.Grade,
                            s.Description,
                            Rank = rank++,
                            ResultText = BuildResult(s) // ✅ Combined string for Excel
                        });
                    }
                }

                byte[] finalResult;
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Student");
                    var currentRow = 1;

                    worksheet.Row(currentRow).Style.Font.Bold = true;

                    // Headers
                    worksheet.Cell(currentRow, 1).Value = "Education";
                    worksheet.Cell(currentRow, 2).Value = "Student Name";
                    worksheet.Cell(currentRow, 3).Value = "Result"; // Combined column
                    worksheet.Cell(currentRow, 4).Value = "Family Name";

                    // Data rows
                    for (int i = 0; i < rankedList.Count; i++)
                    {
                        currentRow++;

                        var s = rankedList[i];

                        // Education
                        worksheet.Cell(currentRow, 1).Value = s.EducationGu + Environment.NewLine + s.Education;
                        worksheet.Cell(currentRow, 1).Style.Alignment.WrapText = true;

                        // Student Name (Gujarati + English)
                        worksheet.Cell(currentRow, 2).Value = s.StudentNameGu + Environment.NewLine + s.StudentName;
                        worksheet.Cell(currentRow, 2).Style.Alignment.WrapText = true;

                        // ✅ Combined Result
                        worksheet.Cell(currentRow, 3).Value = s.ResultText;
                        worksheet.Cell(currentRow, 3).Style.Alignment.WrapText = true;

                        // Family Name
                        worksheet.Cell(currentRow, 4).Value = s.FamilyNameGu + Environment.NewLine + s.Description;
                        worksheet.Cell(currentRow, 4).Style.Alignment.WrapText = true;

                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Borders
                    var dataRange = worksheet.Range(worksheet.Cell(1, 1), worksheet.Cell(currentRow, 4));
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

        // Helper method to format Percentage, Grade, SGPA, CGPA
        private string BuildResult(dynamic s)
        {
            var parts = new List<string>();

            if (s.Percentage != null)
                parts.Add($"{s.Percentage:0.00}%");

            if (!string.IsNullOrEmpty(s.Grade))
                parts.Add(s.Grade);

            if (s.Sgpa != null)
                parts.Add($"SGPA: {s.Sgpa:0.00}");

            if (s.Cgpa != null)
                parts.Add($"CGPA: {s.Cgpa:0.00}");

            return string.Join(", ", parts);
        }


    }
}
