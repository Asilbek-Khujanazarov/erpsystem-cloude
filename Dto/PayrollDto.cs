// DTO lar
public class PayrollCreateDto
{
    public int EmployeeId { get; set; }
    public DateTime PayDate { get; set; }
}

public class PayrollUpdateDto
{
    public DateTime PayDate { get; set; }
}

public class PayrollDto
{
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime PayDate { get; set; }
    public decimal Amount { get; set; }
    public double TotalHoursWorked { get; set; }
    public double PayableHours { get; set; }
    public decimal LatePenalty { get; set; }
    public decimal OvertimeBonus { get; set; }
}

public class EmployeeScheduleUpdateDto
{
    public int EmployeeId { get; set; }
    public decimal HourlyRate { get; set; }
    public string WorkStartTime { get; set; } // "HH:mm"
    public string WorkEndTime { get; set; } // "HH:mm"
}

public class AttendanceDetailDto
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public double HoursWorked { get; set; }
    public double LateHours { get; set; }
    public double OvertimeHours { get; set; }
}
