using HRsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRsystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Har bir model uchun DbSet xususiyatlari
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<TrainingCourse> TrainingCourses { get; set; }
        public DbSet<EmployeeTraining> EmployeeTrainings { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // Model munosabatlari va konfiguratsiyalarni sozlash
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. User bilan Employee o‘rtasidagi 1:1 munosabat
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Employee bilan Department o‘rtasidagi 1:ko‘p munosabat
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Payroll bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. JobApplication bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Employee)
                .WithMany()
                .HasForeignKey(ja => ja.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. EmployeeTraining bilan Employee va TrainingCourse o‘rtasidagi ko‘p-ko‘p munosabat
            modelBuilder.Entity<EmployeeTraining>()
                .HasKey(et => new { et.EmployeeId, et.CourseId });

            modelBuilder.Entity<EmployeeTraining>()
                .HasOne(et => et.Employee)
                .WithMany()
                .HasForeignKey(et => et.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeTraining>()
                .HasOne(et => et.TrainingCourse)
                .WithMany()
                .HasForeignKey(et => et.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Attendance bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}