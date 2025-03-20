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
    public class PayrollController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PayrollController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST: api/payroll (Yangi maosh qo‘shish, Admin uchun)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPayroll([FromBody] PayrollCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                ws.EmployeeId == dto.EmployeeId
            );
            if (schedule == null)
                return BadRequest(new { Message = "Xodim uchun ish jadvali topilmadi." });

            var startOfMonth = new DateTime(dto.PayDate.Year, dto.PayDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var attendances = await _context
                .Attendances.Where(a =>
                    a.EmployeeId == dto.EmployeeId
                    && a.AttendanceDate >= startOfMonth
                    && a.AttendanceDate <= endOfMonth
                    && a.ExitTime.HasValue
                    && a.EntryTime.Date == a.AttendanceDate
                )
                .ToListAsync();

            if (!attendances.Any())
                return BadRequest(new { Message = "Bu oy uchun davomat qaydlari topilmadi." });

            double totalHoursWorked = 0;
            decimal latePenalty = 0;
            decimal overtimeBonus = 0;
            const decimal penaltyPerHour = 10000;
            const decimal bonusPerHour = 15000;

            foreach (var attendance in attendances)
            {
                var hoursWorked = (attendance.ExitTime.Value - attendance.EntryTime).TotalHours;
                totalHoursWorked += hoursWorked;

                var expectedStart = attendance.AttendanceDate.Add(schedule.WorkStartTime);
                var expectedEnd = attendance.AttendanceDate.Add(schedule.WorkEndTime);

                if (attendance.EntryTime > expectedStart)
                {
                    var lateHours = Math.Min(
                        (attendance.EntryTime - expectedStart).TotalHours,
                        schedule.DailyWorkHours
                    );
                    latePenalty += (decimal)lateHours * penaltyPerHour;
                }

                if (attendance.ExitTime > expectedEnd)
                {
                    var overtimeHours = (attendance.ExitTime.Value - expectedEnd).TotalHours;
                    overtimeBonus += (decimal)overtimeHours * bonusPerHour;
                }
            }

            var payableHours = Math.Min(
                totalHoursWorked,
                attendances.Count * schedule.DailyWorkHours
            );
            var baseAmount = (decimal)payableHours * schedule.HourlyRate;
            var finalAmount = baseAmount - latePenalty + overtimeBonus;

            if (finalAmount < 0)
                return BadRequest(
                    new { Message = "Maosh manfiy bo‘lishi mumkin emas. Davomatni tekshiring." }
                );

            var payroll = new Payroll
            {
                EmployeeId = dto.EmployeeId,
                PayDate = dto.PayDate,
                Amount = finalAmount,
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<PayrollDto>(payroll);
            resultDto.TotalHoursWorked = totalHoursWorked;
            resultDto.PayableHours = payableHours;
            resultDto.LatePenalty = latePenalty;
            resultDto.OvertimeBonus = overtimeBonus;
            return CreatedAtAction(nameof(GetMyPayrolls), new { }, resultDto);
        }

        // GET: api/payroll/my-payrolls (Xodimning o‘z maoshlari va davomati)
        [HttpGet("my-payrolls")]
        [Authorize]
        public async Task<IActionResult> GetMyPayrolls(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null
        )
        {
            var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                ws.EmployeeId == employeeId
            );
            if (schedule == null)
                return BadRequest(new { Message = "Ish jadvali topilmadi." });

            var payrollQuery = _context.Payrolls.Where(p => p.EmployeeId == employeeId);
            if (month.HasValue && year.HasValue)
                payrollQuery = payrollQuery.Where(p =>
                    p.PayDate.Month == month && p.PayDate.Year == year
                );

            var payrolls = await payrollQuery.ToListAsync();
            var payrollDtos = _mapper.Map<List<PayrollDto>>(payrolls);

            var attendanceDetails = new List<AttendanceDetailDto>();

            foreach (var payroll in payrollDtos)
            {
                var startOfMonth = new DateTime(payroll.PayDate.Year, payroll.PayDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var attendances = await _context
                    .Attendances.Where(a =>
                        a.EmployeeId == employeeId
                        && a.AttendanceDate >= startOfMonth
                        && a.AttendanceDate <= endOfMonth
                        && a.ExitTime.HasValue
                        && a.EntryTime.Date == a.AttendanceDate
                    )
                    .ToListAsync();

                double totalHoursWorked = 0;
                decimal latePenalty = 0;
                decimal overtimeBonus = 0;
                const decimal penaltyPerHour = 10000;
                const decimal bonusPerHour = 15000;

                foreach (var attendance in attendances)
                {
                    var hoursWorked = (attendance.ExitTime.Value - attendance.EntryTime).TotalHours;
                    totalHoursWorked += hoursWorked;

                    var expectedStart = attendance.AttendanceDate.Add(schedule.WorkStartTime);
                    var expectedEnd = attendance.AttendanceDate.Add(schedule.WorkEndTime);

                    double lateHours = 0;
                    double overtimeHours = 0;

                    if (attendance.EntryTime > expectedStart)
                    {
                        lateHours = Math.Min(
                            (attendance.EntryTime - expectedStart).TotalHours,
                            schedule.DailyWorkHours
                        );
                        latePenalty += (decimal)lateHours * penaltyPerHour;
                    }

                    if (attendance.ExitTime > expectedEnd)
                    {
                        overtimeHours = (attendance.ExitTime.Value - expectedEnd).TotalHours;
                        overtimeBonus += (decimal)overtimeHours * bonusPerHour;
                    }

                    attendanceDetails.Add(
                        new AttendanceDetailDto
                        {
                            Date = attendance.AttendanceDate,
                            EntryTime = attendance.EntryTime,
                            ExitTime = attendance.ExitTime.Value,
                            HoursWorked = hoursWorked,
                            LateHours = lateHours,
                            OvertimeHours = overtimeHours,
                        }
                    );
                }

                payroll.TotalHoursWorked = totalHoursWorked;
                payroll.PayableHours = Math.Min(
                    totalHoursWorked,
                    attendances.Count * schedule.DailyWorkHours
                );
                payroll.LatePenalty = latePenalty;
                payroll.OvertimeBonus = overtimeBonus;
            }

            var totalAmount = payrollDtos.Sum(p => p.Amount);
            return Ok(
                new
                {
                    HourlyRate = schedule.HourlyRate,
                    WorkStartTime = schedule.WorkStartTime.ToString(@"hh\:mm"),
                    WorkEndTime = schedule.WorkEndTime.ToString(@"hh\:mm"),
                    DailyWorkHours = schedule.DailyWorkHours,
                    Payrolls = payrollDtos,
                    AttendanceDetails = attendanceDetails,
                    TotalAmount = totalAmount,
                }
            );
        }

        // GET: api/payroll (Barcha maoshlar va statistika, Admin uchun)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayrolls(
            [FromQuery] int? employeeId = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null
        )
        {
            var query = _context.Payrolls.Include(p => p.Employee).AsQueryable();
            if (employeeId.HasValue)
                query = query.Where(p => p.EmployeeId == employeeId.Value);
            if (month.HasValue && year.HasValue)
                query = query.Where(p => p.PayDate.Month == month && p.PayDate.Year == year);

            var payrolls = await query.ToListAsync();
            var payrollDtos = _mapper.Map<List<PayrollDto>>(payrolls);
            var attendanceDetails = new List<AttendanceDetailDto>();

            foreach (var payroll in payrollDtos)
            {
                var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                    ws.EmployeeId == payroll.EmployeeId
                );
                if (schedule == null)
                    continue; // Agar jadval topilmasa, o‘tkazib yuboramiz

                var startOfMonth = new DateTime(payroll.PayDate.Year, payroll.PayDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var attendances = await _context
                    .Attendances.Where(a =>
                        a.EmployeeId == payroll.EmployeeId
                        && a.AttendanceDate >= startOfMonth
                        && a.AttendanceDate <= endOfMonth
                        && a.ExitTime.HasValue
                        && a.EntryTime.Date == a.AttendanceDate
                    )
                    .ToListAsync();

                double totalHoursWorked = 0;
                decimal latePenalty = 0;
                decimal overtimeBonus = 0;
                const decimal penaltyPerHour = 10000;
                const decimal bonusPerHour = 15000;

                foreach (var attendance in attendances)
                {
                    var hoursWorked = (attendance.ExitTime.Value - attendance.EntryTime).TotalHours;
                    totalHoursWorked += hoursWorked;

                    var expectedStart = attendance.AttendanceDate.Add(schedule.WorkStartTime);
                    var expectedEnd = attendance.AttendanceDate.Add(schedule.WorkEndTime);

                    double lateHours = 0;
                    double overtimeHours = 0;

                    if (attendance.EntryTime > expectedStart)
                    {
                        lateHours = Math.Min(
                            (attendance.EntryTime - expectedStart).TotalHours,
                            schedule.DailyWorkHours
                        );
                        latePenalty += (decimal)lateHours * penaltyPerHour;
                    }

                    if (attendance.ExitTime > expectedEnd)
                    {
                        overtimeHours = (attendance.ExitTime.Value - expectedEnd).TotalHours;
                        overtimeBonus += (decimal)overtimeHours * bonusPerHour;
                    }

                    attendanceDetails.Add(
                        new AttendanceDetailDto
                        {
                            EmployeeId = payroll.EmployeeId,
                            EmployeeName = payroll.EmployeeName,
                            Date = attendance.AttendanceDate,
                            EntryTime = attendance.EntryTime,
                            ExitTime = attendance.ExitTime.Value,
                            HoursWorked = hoursWorked,
                            LateHours = lateHours,
                            OvertimeHours = overtimeHours,
                        }
                    );
                }

                payroll.TotalHoursWorked = totalHoursWorked;
                payroll.PayableHours = Math.Min(
                    totalHoursWorked,
                    attendances.Count * schedule.DailyWorkHours
                );
                payroll.LatePenalty = latePenalty;
                payroll.OvertimeBonus = overtimeBonus;
            }

            var totalAmount = payrollDtos.Sum(p => p.Amount);
            var totalLatePenalty = payrollDtos.Sum(p => p.LatePenalty);
            var totalOvertimeBonus = payrollDtos.Sum(p => p.OvertimeBonus);

            return Ok(
                new
                {
                    Payrolls = payrollDtos,
                    AttendanceDetails = attendanceDetails,
                    TotalAmount = totalAmount,
                    TotalLatePenalty = totalLatePenalty,
                    TotalOvertimeBonus = totalOvertimeBonus,
                }
            );
        }

        // PUT: api/payroll/update-schedule (Ish rejimini yangilash, Admin uchun)
        [HttpPut("update-schedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateWorkSchedule(
            [FromBody] EmployeeScheduleUpdateDto dto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                ws.EmployeeId == dto.EmployeeId
            );
            if (schedule == null)
            {
                schedule = new WorkSchedule { EmployeeId = dto.EmployeeId };
                _context.WorkSchedules.Add(schedule);
            }

            schedule.HourlyRate = dto.HourlyRate;
            schedule.WorkStartTime = TimeSpan.Parse(dto.WorkStartTime);
            schedule.WorkEndTime = TimeSpan.Parse(dto.WorkEndTime);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/payroll/{id} (Maoshni yangilash, Admin uchun)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePayroll(int id, [FromBody] PayrollUpdateDto dto)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null)
                return NotFound(new { Message = "Maosh qaydi topilmadi." });

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                ws.EmployeeId == payroll.EmployeeId
            );
            if (schedule == null)
                return BadRequest(new { Message = "Ish jadvali topilmadi." });

            var startOfMonth = new DateTime(dto.PayDate.Year, dto.PayDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var attendances = await _context
                .Attendances.Where(a =>
                    a.EmployeeId == payroll.EmployeeId
                    && a.AttendanceDate >= startOfMonth
                    && a.AttendanceDate <= endOfMonth
                    && a.ExitTime.HasValue
                    && a.EntryTime.Date == a.AttendanceDate
                )
                .ToListAsync();

            if (!attendances.Any())
                return BadRequest(new { Message = "Bu oy uchun davomat qaydlari topilmadi." });

            double totalHoursWorked = 0;
            decimal latePenalty = 0;
            decimal overtimeBonus = 0;
            const decimal penaltyPerHour = 10000;
            const decimal bonusPerHour = 15000;

            foreach (var attendance in attendances)
            {
                var hoursWorked = (attendance.ExitTime.Value - attendance.EntryTime).TotalHours;
                totalHoursWorked += hoursWorked;

                var expectedStart = attendance.AttendanceDate.Add(schedule.WorkStartTime);
                var expectedEnd = attendance.AttendanceDate.Add(schedule.WorkEndTime);

                if (attendance.EntryTime > expectedStart)
                {
                    var lateHours = Math.Min(
                        (attendance.EntryTime - expectedStart).TotalHours,
                        schedule.DailyWorkHours
                    );
                    latePenalty += (decimal)lateHours * penaltyPerHour;
                }

                if (attendance.ExitTime > expectedEnd)
                {
                    var overtimeHours = (attendance.ExitTime.Value - expectedEnd).TotalHours;
                    overtimeBonus += (decimal)overtimeHours * bonusPerHour;
                }
            }

            var payableHours = Math.Min(
                totalHoursWorked,
                attendances.Count * schedule.DailyWorkHours
            );
            var baseAmount = (decimal)payableHours * schedule.HourlyRate;
            var finalAmount = baseAmount - latePenalty + overtimeBonus;

            if (finalAmount < 0)
                return BadRequest(
                    new { Message = "Maosh manfiy bo‘lishi mumkin emas. Davomatni tekshiring." }
                );

            payroll.PayDate = dto.PayDate;
            payroll.Amount = finalAmount;
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<PayrollDto>(payroll);
            resultDto.TotalHoursWorked = totalHoursWorked;
            resultDto.PayableHours = payableHours;
            resultDto.LatePenalty = latePenalty;
            resultDto.OvertimeBonus = overtimeBonus;
            return Ok(resultDto);
        }

        // DELETE: api/payroll/{id} (Maoshni o‘chirish, Admin uchun)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayroll(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null)
                return NotFound(new { Message = "Maosh qaydi topilmadi." });

            _context.Payrolls.Remove(payroll);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/payroll/attendance-report (Davomat hisoboti)
        [HttpGet("attendance-report")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceReport(
            [FromQuery] int? employeeId,
            [FromQuery] int month,
            [FromQuery] int year
        )
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var isAdmin = User.IsInRole("Admin");
            var targetEmployeeId =
                isAdmin && employeeId.HasValue ? employeeId.Value : currentUserId;

            var employee = await _context.Employees.FindAsync(targetEmployeeId);
            if (employee == null)
                return NotFound(new { Message = "Xodim topilmadi." });

            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws =>
                ws.EmployeeId == targetEmployeeId
            );
            if (schedule == null)
                return BadRequest(new { Message = "Ish jadvali topilmadi." });

            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var attendances = await _context
                .Attendances.Where(a =>
                    a.EmployeeId == targetEmployeeId
                    && a.AttendanceDate >= startOfMonth
                    && a.AttendanceDate <= endOfMonth
                    && a.ExitTime.HasValue
                    && a.EntryTime.Date == a.AttendanceDate
                )
                .ToListAsync();

            var report = new List<AttendanceDetailDto>();
            double totalHoursWorked = 0;
            decimal latePenalty = 0;
            decimal overtimeBonus = 0;
            const decimal penaltyPerHour = 10000;
            const decimal bonusPerHour = 15000;

            foreach (var attendance in attendances)
            {
                var hoursWorked = (attendance.ExitTime.Value - attendance.EntryTime).TotalHours;
                totalHoursWorked += hoursWorked;

                var expectedStart = attendance.AttendanceDate.Add(schedule.WorkStartTime);
                var expectedEnd = attendance.AttendanceDate.Add(schedule.WorkEndTime);

                double lateHours = 0;
                double overtimeHours = 0;

                if (attendance.EntryTime > expectedStart)
                {
                    lateHours = Math.Min(
                        (attendance.EntryTime - expectedStart).TotalHours,
                        schedule.DailyWorkHours
                    );
                    latePenalty += (decimal)lateHours * penaltyPerHour;
                }

                if (attendance.ExitTime > expectedEnd)
                {
                    overtimeHours = (attendance.ExitTime.Value - expectedEnd).TotalHours;
                    overtimeBonus += (decimal)overtimeHours * bonusPerHour;
                }

                report.Add(
                    new AttendanceDetailDto
                    {
                        EmployeeId = targetEmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        Date = attendance.AttendanceDate,
                        EntryTime = attendance.EntryTime,
                        ExitTime = attendance.ExitTime.Value,
                        HoursWorked = hoursWorked,
                        LateHours = lateHours,
                        OvertimeHours = overtimeHours,
                    }
                );
            }

            return Ok(
                new
                {
                    EmployeeId = targetEmployeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    HourlyRate = schedule.HourlyRate,
                    WorkStartTime = schedule.WorkStartTime.ToString(@"hh\:mm"),
                    WorkEndTime = schedule.WorkEndTime.ToString(@"hh\:mm"),
                    DailyWorkHours = schedule.DailyWorkHours,
                    AttendanceReport = report,
                    TotalHoursWorked = totalHoursWorked,
                    TotalLatePenalty = latePenalty,
                    TotalOvertimeBonus = overtimeBonus,
                }
            );
        }
    }
}
