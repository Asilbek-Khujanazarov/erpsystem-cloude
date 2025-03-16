namespace HRsystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }              // Primary Key (PK)
       public required string UserName { get; set; } // Username o‘rniga UserName
        public required string PasswordHash { get; set; } // Hash qilingan parol
        public required string Role { get; set; }        // "Admin" yoki "Employee"
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;
        public DateTime HireDate { get; set; } = DateTime.MinValue;
        public int DepartmentId { get; set; }
        public required string Position { get; set; }
        public decimal Salary { get; set; } = 0.0m;

        public Department Department { get; set; } = null!; // Navigatsion xususiyat
    }
}