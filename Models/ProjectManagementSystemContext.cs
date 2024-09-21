using Microsoft.EntityFrameworkCore;

namespace ProjectManagementSystem.Models
{
    public class ProjectManagementSystemContext : DbContext
    {
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }

        public ProjectManagementSystemContext()
        {
            Database.EnsureCreated();
        }

        public ProjectManagementSystemContext(DbContextOptions<ProjectManagementSystemContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Surname).HasMaxLength(50);
                entity.Property(e => e.FullName)
                    .HasComputedColumnSql("[Name] + ' ' + [Surname]");

            });
        }
    }
}
