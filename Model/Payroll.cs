namespace HRsystem.Models
{
    public class Payroll
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PayDate { get; set; }
        public decimal Amount { get; set; }
        public Employee Employee { get; set; }
    }
}