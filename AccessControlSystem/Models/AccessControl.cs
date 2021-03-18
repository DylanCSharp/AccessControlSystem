using Microsoft.EntityFrameworkCore;

#nullable disable

namespace AccessControlSystem.Models
{
    public partial class AccessControl : DbContext
    {
        public AccessControl()
        {
        }

        public AccessControl(DbContextOptions<AccessControl> options)
            : base(options)
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeLog> EmployeeLogs { get; set; }
        public virtual DbSet<VisitorsLog> VisitorsLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("EMPLOYEES");

                entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");

                entity.Property(e => e.EmployeeAddress)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("Employee_Address");

                entity.Property(e => e.EmployeeHashCode)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Employee_Name");
            });

            modelBuilder.Entity<EmployeeLog>(entity =>
            {
                entity.HasKey(e => e.EmployeeLogNumber)
                    .HasName("PK__EMPLOYEE__CA37BC1F33BC2996");

                entity.ToTable("EMPLOYEE_LOG");

                entity.Property(e => e.EmployeeLogNumber).HasColumnName("Employee_Log_Number");

                entity.Property(e => e.CheckInStatus).HasColumnName("Check_In_Status");

                entity.Property(e => e.DateLog)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Date_Log");

                entity.Property(e => e.EmployeeId).HasColumnName("Employee_ID");

                entity.Property(e => e.TimeIn)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Time_In");

                entity.Property(e => e.TimeOut)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Time_Out");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeLogs)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__EMPLOYEE___Emplo__286302EC");
            });

            modelBuilder.Entity<VisitorsLog>(entity =>
            {
                entity.HasKey(e => e.VisitLogNumber)
                    .HasName("PK__VISITORS__068A4B49E3653466");

                entity.ToTable("VISITORS_LOG");

                entity.Property(e => e.VisitLogNumber).HasColumnName("Visit_Log_Number");

                entity.Property(e => e.VisitorName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Visitor_Name");

                entity.Property(e => e.VisitorPictureUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Visitor_Picture_URL");

                entity.Property(e => e.VisitorReason)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Visitor_Reason");

                entity.Property(e => e.VisitorSeeingWhom)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Visitor_SeeingWhom");

                entity.Property(e => e.VisitorTimestamp)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Visitor_Timestamp");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
