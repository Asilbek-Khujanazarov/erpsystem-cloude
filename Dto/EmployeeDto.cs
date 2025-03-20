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
    public int UserId { get; set; }
}

