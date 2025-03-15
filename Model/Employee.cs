namespace HRsystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;
        public DateTime HireDate { get; set; } = DateTime.MinValue;
        public int DepartmentId { get; set; }
        public required string Position { get; set; }
        public decimal Salary { get; set; } = 0.0m;
        public int UserId { get; set; }
        public string? Role { get; set; }
        public Department Department { get; set; } = null!; // Nullable qilindi
    }
}