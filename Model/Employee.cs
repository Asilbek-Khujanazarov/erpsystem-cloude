public class Employee
{
    public int EmployeeId { get; set; }          // Primary Key (PK)
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }        // Foreign Key (FK) - Department jadvaliga
    public required string Position { get; set; }
    public decimal Salary { get; set; }
}