using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Infrastructure.Data.Context
{
    /// <summary>
    /// Database context for the task management application
    /// </summary>
    public class TaskManagementDbContext : DbContext
    {
        /// <summary>
        /// Tasks table
        /// </summary>
        public DbSet<Domain.Entities.Task> Tasks { get; set; }

        /// <summary>
        /// Creates a new database context
        /// </summary>
        /// <param name="options">Context options</param>
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the database model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Domain.Entities.Task>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Description)
                      .HasMaxLength(1000);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedAt)
                      .IsRequired();
            });
        }
    }
}
