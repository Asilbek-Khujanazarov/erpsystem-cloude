public class JobApplication
{
    public int ApplicationId { get; set; }       // PK
    public int EmployeeId { get; set; }          // FK - Employee
    public required string PositionAppliedFor { get; set; }
    public DateTime ApplicationDate { get; set; }
    public required string Status { get; set; }
}