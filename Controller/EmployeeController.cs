using HRsystem.Data;
using HRsystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRsystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public EmployeeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/employee/register (Ro‘yxatdan o‘tish)
        [HttpPost("register")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] EmployeeRegisterDto dto)
        {
            if (_context.Employees.Any(e => e.UserName == dto.UserName))
                return BadRequest(new { Message = "Bu username allaqachon mavjud." });

            if (!_context.Departments.Any(d => d.DepartmentId == dto.DepartmentId))
                return BadRequest(new { Message = "Bunday bo‘lim topilmadi." });

            var employee = new Employee
            {
                UserName = dto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role ?? "Employee", // Agar rol kiritilmasa, standart "Employee"
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                Position = dto.Position,
                Salary = dto.Salary,
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xodim muvaffaqiyatli ro‘yxatdan o‘tdi.", EmployeeId = employee.EmployeeId });
        }

        // POST: api/employee/login (Kirish)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EmployeeLoginDto dto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == dto.UserName);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
                return Unauthorized(new { Message = "Noto‘g‘ri username yoki parol." });

            var token = GenerateJwtToken(employee);
            return Ok(new { EmployeeId = employee.EmployeeId, Username = employee.UserName, Role = employee.Role, Token = token });
        }

        // GET: api/employee/my-info (O‘z ma'lumotlarini ko‘rish)
        [HttpGet("my-info")]
        [Authorize]
        public async Task<IActionResult> GetMyInfo()
        {
            var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    UserName = e.UserName,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    DateOfBirth = e.DateOfBirth,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    Position = e.Position,
                    Salary = e.Salary,
                    Role = e.Role
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            return Ok(employee);
        }

        // GET: api/employee (Barcha xodimlarni ko‘rish, faqat Admin uchun)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllEmployees([FromQuery] string? role = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(e => e.Role == role);

            var employees = await query
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    UserName = e.UserName,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    DateOfBirth = e.DateOfBirth,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    Position = e.Position,
                    Salary = e.Salary,
                    Role = e.Role
                })
                .ToListAsync();

            return Ok(employees);
        }

        // JWT token generatsiyasi
        private string GenerateJwtToken(Employee employee)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.UserName),
                new Claim(ClaimTypes.Role, employee.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTO lar
    public class EmployeeRegisterDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; } // Optional, default "Employee"
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public required string Position { get; set; }
        public decimal Salary { get; set; }
    }

    public class EmployeeLoginDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public required string Position { get; set; }
        public decimal Salary { get; set; }
        public required string Role { get; set; }
    }
}