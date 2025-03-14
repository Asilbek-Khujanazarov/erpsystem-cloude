public class User
{
    public int UserId { get; set; }              // Primary Key (PK)
    public required string Username { get; set; }
    public required string PasswordHash { get; set; } // Parol xavfsiz saqlanadi
    public int EmployeeId { get; set; }          // Foreign Key (FK) - Employee jadvaliga bog‘lanadi
    public required string Role { get; set; }    // "Admin" yoki "Employee"
}