using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Faculty> Faculty => Set<Faculty>();
    public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(x => x.StudentId);

            entity.Property(x => x.StudentId)
                .UseIdentityColumn();

            entity.Property(x => x.StudentNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.StudentNumber)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.ToTable("Faculty");
            entity.HasKey(x => x.FacultyId);

            entity.Property(x => x.FacultyId)
                .UseIdentityColumn();

            entity.Property(x => x.FacultyNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.FacultyNumber)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<StudentGrade>(entity =>
        {
            entity.ToTable("StudentGrades", table =>
            {
                table.HasCheckConstraint(
                    "CK_StudentGrades_Grade",
                    "[Grade] BETWEEN 0 AND 100");
            });

            entity.HasKey(x => x.GradeId);

            entity.Property(x => x.GradeId)
                .UseIdentityColumn();

            entity.Property(x => x.SubjectCode)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.SubjectName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Remarks)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Semester)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SchoolYear)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.DatePosted)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.DateModified)
                .HasColumnType("datetime2(0)");

            entity.HasOne(x => x.Student)
                .WithMany(x => x.Grades)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Faculty)
                .WithMany(x => x.Grades)
                .HasForeignKey(x => x.FacultyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(x => new
            {
                x.StudentId,
                x.SubjectCode,
                x.Semester,
                x.SchoolYear
            })
                .IsUnique();

            entity.HasIndex(x => x.StudentId);
            entity.HasIndex(x => x.FacultyId);
        });
    }
}