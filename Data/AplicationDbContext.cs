using HRsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRsystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // Model munosabatlari va konfiguratsiyalarni sozlash
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 2. Employee bilan Department o‘rtasidagi 1:ko‘p munosabat
            modelBuilder
                .Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>().HasIndex(e => e.UserName).IsUnique();

            modelBuilder.Entity<Employee>().Property(e => e.UserName).IsRequired();
            modelBuilder.Entity<Employee>().Property(e => e.PasswordHash).IsRequired();
            modelBuilder.Entity<Employee>().Property(e => e.Role).IsRequired();
            modelBuilder.Entity<Employee>().Property(e => e.FirstName).IsRequired();
            modelBuilder.Entity<Employee>().Property(e => e.LastName).IsRequired();
            modelBuilder.Entity<Employee>().Property(e => e.Position).IsRequired();

            modelBuilder.Entity<Department>().Property(d => d.DepartmentName).IsRequired();

            // 3. Payroll bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder
                .Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. JobApplication bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder.Entity<JobApplication>().HasKey(j => j.ApplicationId); // Primary Key
            modelBuilder
                .Entity<JobApplication>()
                .Property(ja => ja.PositionAppliedFor)
                .IsRequired();
            modelBuilder.Entity<JobApplication>().Property(ja => ja.Status).IsRequired();

            // 6. Attendance bilan Employee o‘rtasidagi 1:ko‘p munosabat
            modelBuilder
                .Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
