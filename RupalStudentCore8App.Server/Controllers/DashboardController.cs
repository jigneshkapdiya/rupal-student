using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RupalStudentCore8App.Server.Data;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashboardController : ControllerBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ApplicationDbContext _Db;

        public DashboardController(ApplicationDbContext Db)
        {
            _Db = Db;
        }

        [HttpGet("GetDashboardList")]
        public async Task<IActionResult> GetDashboardList()
        {
            try
            {
                var allStudents = await _Db.StudentMarkSheets.CountAsync();
                var approvedStudents = await _Db.StudentMarkSheets.Where(w => w.Status == StudentStatus.Approved).CountAsync();
                var rejectedStudents = await _Db.StudentMarkSheets.Where(w => w.Status == StudentStatus.Rejected).CountAsync();
                var pendingStudents = await _Db.StudentMarkSheets.Where(w => w.Status == StudentStatus.New).CountAsync();

                return Ok(new
                {
                    allStudents,
                    approvedStudents,
                    rejectedStudents,
                    pendingStudents
                });
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return BadRequest("Fail to get data.");
            }
        }

        [HttpGet("GetEducationWithStudentCount")]
        public async Task<IActionResult> GetEducationWithStudentCount()
        {
            try
            {
                var result = await (from edu in _Db.StudentEducations
                                    join stu in _Db.StudentMarkSheets
                                    on edu.Name equals stu.Education into studentGroup
                                    select new
                                    {
                                        edu.Id,
                                        edu.Name,
                                        edu.NameGu,
                                        StudentCount = studentGroup.Count()
                                    }).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", error = ex.Message });
            }
        }

    }
}
