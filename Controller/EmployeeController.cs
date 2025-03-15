using HRsystem.Data;
using HRsystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRsystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeeController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public EmployeeController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet("my-info")]
		[Authorize]
		public async Task<IActionResult> GetMyInfo([FromQuery] int userId)
		{
			var employee = await _context.Employees
				.Include(e => e.Department)
				.Where(e => e.UserId == userId)
				.Select(e => new EmployeeDto
				{
					EmployeeId = e.EmployeeId,
					FirstName = e.FirstName,
					LastName = e.LastName,
					DateOfBirth = e.DateOfBirth,
					HireDate = e.HireDate,
					DepartmentId = e.DepartmentId,
					DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
					Position = e.Position,
					Salary = e.Salary,
					UserId = e.UserId
				})
				.FirstOrDefaultAsync();

			if (employee == null)
			{
				return NotFound(new { Message = "Xodim topilmadi" });
			}

			return Ok(employee);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllEmployees([FromQuery] string? role = null)
		{
			var query = _context.Employees
				.Include(e => e.Department)
				.AsQueryable();

			if (!string.IsNullOrEmpty(role))
			{
				query = query.Where(e => e.Role == role);
			}

			var employees = await query
				.Select(e => new EmployeeDto
				{
					EmployeeId = e.EmployeeId,
					FirstName = e.FirstName,
					LastName = e.LastName,
					DateOfBirth = e.DateOfBirth,
					HireDate = e.HireDate,
					DepartmentId = e.DepartmentId,
					DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
					Position = e.Position,
					Salary = e.Salary,
					UserId = e.UserId
				})
				.ToListAsync();

			return Ok(employees);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddEmployee([FromBody] EmployeeDto employeeDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var employee = new Employee
			{
				FirstName = employeeDto.FirstName,
				LastName = employeeDto.LastName,
				DateOfBirth = employeeDto.DateOfBirth,
				HireDate = employeeDto.HireDate,
				DepartmentId = employeeDto.DepartmentId,
				Position = employeeDto.Position,
				Salary = employeeDto.Salary,
				UserId = employeeDto.UserId,
				Role = "Employee"
			};

			_context.Employees.Add(employee);
			await _context.SaveChangesAsync();

			employeeDto.EmployeeId = employee.EmployeeId;
			return CreatedAtAction(nameof(GetMyInfo), new { userId = employee.UserId }, employeeDto);
		}
	}

	public class EmployeeDto
	{
		public int EmployeeId { get; set; }
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public DateTime DateOfBirth { get; set; }
		public DateTime HireDate { get; set; }
		public int DepartmentId { get; set; }
		public string? DepartmentName { get; set; }
		public required string Position { get; set; }
		public decimal Salary { get; set; }
		public int UserId { get; set; }
	}
}