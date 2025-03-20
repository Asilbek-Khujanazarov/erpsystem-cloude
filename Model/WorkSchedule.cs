namespace HRsystem.Models
{
    public class WorkSchedule
    {
        public int WorkScheduleId { get; set; } // Primary Key
        public int EmployeeId { get; set; } // Foreign Key
        public decimal HourlyRate { get; set; } // Soatlik stavka
        public TimeSpan WorkStartTime { get; set; } // Ish boshlanish vaqti
        public TimeSpan WorkEndTime { get; set; } // Ish tugash vaqti
        public double DailyWorkHours => (WorkEndTime - WorkStartTime).TotalHours; // Hisoblanadi
        public Employee Employee { get; set; } // Bog‘lanish
    }
}