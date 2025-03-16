using HRsystem.Data;
using HRsystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HRsystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/department (Barcha bo‘limlarni ko‘rish, faqat Admin uchun)
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName
                })
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/department/{id} (Muayyan bo‘limni ko‘rish, faqat Admin uchun)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Where(d => d.DepartmentId == id)
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName
                })
                .FirstOrDefaultAsync();

            if (department == null)
                return NotFound(new { Message = "Bo‘lim topilmadi." });

            return Ok(department);
        }

        // POST: api/department (Bo‘lim qo‘shish, faqat Admin uchun)
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDepartment([FromBody] DepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var department = new Department
            {
                DepartmentName = dto.DepartmentName
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            dto.DepartmentId = department.DepartmentId;
            return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, dto);
        }
    }

    // DTO
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
    }
}