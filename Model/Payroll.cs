public class Payroll
{
    public int PayrollId { get; set; }           // PK
    public int EmployeeId { get; set; }          // FK - Employee
    public DateTime PayDate { get; set; }
    public decimal Amount { get; set; }
}