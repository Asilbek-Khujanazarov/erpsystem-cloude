
using HRsystem.Data;
using HRsystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRsystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/jobapplication (Yangi ariza qo‘shish)
        [HttpPost]
        [Authorize] // Har qanday autentifikatsiya qilingan foydalanuvchi
        public async Task<IActionResult> AddJobApplication([FromBody] JobApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var jobApplication = new JobApplication
            {
                EmployeeId = employeeId,
                PositionAppliedFor = dto.PositionAppliedFor,
                ApplicationDate = DateTime.UtcNow, // Hozirgi vaqt
                Status = "Pending" // Standart holat
            };

            _context.JobApplications.Add(jobApplication);
            await _context.SaveChangesAsync();

            dto.ApplicationId = jobApplication.ApplicationId;
            dto.EmployeeId = jobApplication.EmployeeId;
            dto.ApplicationDate = jobApplication.ApplicationDate;
            dto.Status = jobApplication.Status;

            return CreatedAtAction(nameof(GetMyApplications), new { }, dto);
        }

        // GET: api/jobapplication/my-applications (Xodimning o‘z arizalarini ko‘rish)
        [HttpGet("my-applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var applications = await _context.JobApplications
                .Where(ja => ja.EmployeeId == employeeId)
                .Select(ja => new JobApplicationDto
                {
                    ApplicationId = ja.ApplicationId,
                    EmployeeId = ja.EmployeeId,
                    PositionAppliedFor = ja.PositionAppliedFor,
                    ApplicationDate = ja.ApplicationDate,
                    Status = ja.Status
                })
                .ToListAsync();

            return Ok(applications);
        }

        // GET: api/jobapplication (Barcha arizalarni ko‘rish, faqat Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllJobApplications()
        {
            var applications = await _context.JobApplications
                .Include(ja => ja.Employee)
                .Select(ja => new JobApplicationDto
                {
                    ApplicationId = ja.ApplicationId,
                    EmployeeId = ja.EmployeeId,
                    PositionAppliedFor = ja.PositionAppliedFor,
                    ApplicationDate = ja.ApplicationDate,
                    Status = ja.Status,
                    EmployeeName = $"{ja.Employee.FirstName} {ja.Employee.LastName}"
                })
                .ToListAsync();

            return Ok(applications);
        }

        // PUT: api/jobapplication/{id} (Ariza holatini yangilash, faqat Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateJobApplication(int id, [FromBody] JobApplicationUpdateDto dto)
        {
            var application = await _context.JobApplications.FindAsync(id);
            if (application == null)
                return NotFound(new { Message = "Ariza topilmadi." });

            application.Status = dto.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO lar
    public class JobApplicationDto
    {
        public int ApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public required string PositionAppliedFor { get; set; }
        public DateTime ApplicationDate { get; set; }
        public required string Status { get; set; }
        public string? EmployeeName { get; set; } // Admin uchun qo‘shimcha
    }

    public class JobApplicationUpdateDto
    {
        public required string Status { get; set; } // Faqat holatni yangilash uchun
    }
}