using System.Security.Claims;
using AutoMapper;
using HRsystem.Data;
using HRsystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRsystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AttendanceController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST: api/attendance/entry (Ishga kirish vaqti, EmployeeId bilan)
        [HttpPost("entry")]
        public async Task<IActionResult> RecordEntry([FromBody] AttendanceEntryDto dto)
        {
            if (!ModelState.IsValid || dto.EmployeeId <= 0)
                return BadRequest(new { Message = "EmployeeId noto‘g‘ri kiritildi." });

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var today = DateTime.UtcNow.Date;
            var existingAttendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.EmployeeId == dto.EmployeeId && a.AttendanceDate == today
            );

            if (existingAttendance != null)
                return BadRequest(new { Message = "Bu xodim bugun allaqachon kirish qayd etgan." });

            var attendance = _mapper.Map<Attendance>(dto); // DTO dan Attendance ga xaritalash
            attendance.EntryTime = DateTime.UtcNow; // Dinamik qiymatlar
            attendance.AttendanceDate = today;

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    Message = "Kirish vaqti qayd etildi.",
                    AttendanceId = attendance.AttendanceId,
                }
            );
        }

        // POST: api/attendance/exit (Ishdan chiqish vaqti, EmployeeId bilan)
        [HttpPost("exit")]
        public async Task<IActionResult> RecordExit([FromBody] AttendanceEntryDto dto)
        {
            if (!ModelState.IsValid || dto.EmployeeId <= 0)
                return BadRequest(new { Message = "EmployeeId noto‘g‘ri kiritildi." });

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.EmployeeId == dto.EmployeeId && a.AttendanceDate == today && a.ExitTime == null
            );

            if (attendance == null)
                return BadRequest(
                    new
                    {
                        Message = "Bugungi kirish qaydi topilmadi yoki chiqish allaqachon qayd etilgan.",
                    }
                );

            attendance.ExitTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var dtoResult = _mapper.Map<AttendanceDto>(attendance); // Attendance dan DTO ga xaritalash
            return Ok(new { Message = "Chiqish vaqti qayd etildi.", Attendance = dtoResult });
        }

        // GET: api/attendance/my-attendance (Xodimning o‘z davomati, token bilan)
        [HttpGet("my-attendance")]
        // [Authorize]
        public async Task<IActionResult> GetMyAttendance(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null
        )
        {
            var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var query = _context.Attendances.Where(a => a.EmployeeId == employeeId);

            if (month.HasValue && year.HasValue)
                query = query.Where(a =>
                    a.AttendanceDate.Month == month && a.AttendanceDate.Year == year
                );

            var attendances = await query.ToListAsync();
            var attendanceDtos = _mapper.Map<List<AttendanceDto>>(attendances);

            var totalHours = attendanceDtos.Sum(a => a.HoursWorked);
            var totalDays = attendanceDtos.Count(a => a.ExitTime.HasValue);

            return Ok(
                new
                {
                    Attendances = attendanceDtos,
                    TotalDaysWorked = totalDays,
                    TotalHoursWorked = totalHours,
                }
            );
        }

        // GET: api/attendance (Admin uchun barcha davomat)
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttendance(
            [FromQuery] int? employeeId = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null
        )
        {
            var query = _context.Attendances.Include(a => a.Employee).AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (month.HasValue && year.HasValue)
                query = query.Where(a =>
                    a.AttendanceDate.Month == month && a.AttendanceDate.Year == year
                );

            var attendances = await query.ToListAsync();
            var attendanceDtos = _mapper.Map<List<AttendanceDto>>(attendances);

            var totalHours = attendanceDtos.Sum(a => a.HoursWorked);
            var totalDays = attendanceDtos.Count(a => a.ExitTime.HasValue);

            return Ok(
                new
                {
                    Attendances = attendanceDtos,
                    TotalDaysWorked = totalDays,
                    TotalHoursWorked = totalHours,
                }
            );
        }
    }
}
