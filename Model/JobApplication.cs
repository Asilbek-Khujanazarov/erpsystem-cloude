using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRsystem.Models
{
    public class JobApplication
    {
        [Key]  // Explicit Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int ApplicationId { get; set; }

        public int EmployeeId { get; set; }  // FK - Employee

        public required string PositionAppliedFor { get; set; }
        public DateTime ApplicationDate { get; set; }
        public required string Status { get; set; }

        public Employee Employee { get; set; } // Navigation Property
    }
}
