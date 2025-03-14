public class TrainingCourse
{
    public int CourseId { get; set; }            // PK
    public required string CourseName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class EmployeeTraining
{
    public int EmployeeId { get; set; }          // FK - Employee
    public int CourseId { get; set; }            // FK - TrainingCourse
}

    



