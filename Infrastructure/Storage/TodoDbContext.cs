using Microsoft.EntityFrameworkCore;
using TODO_List.Domain.Entities;

namespace TODO_List.Infrastructure.Storage
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }
        public DbSet<TaskDbModel> Tasks {  get; set; }
        public DbSet<SubTaskDbModel> SubTasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskDbModel>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.TaskId)
                      .HasColumnName("TaskId").
                       ValueGeneratedOnAdd();

                entity.Property(e => e.TitleName)
                      .HasColumnName("TitleName");

                entity.Property(e => e.IsCompleted)
                      .HasColumnName("IsCompleted");
            });

            modelBuilder.Entity<SubTaskDbModel>(entity =>
            {
                entity.ToTable("SubTasks");

                entity.HasKey(e => e.SubTaskId);

                entity.Property(e => e.SubTaskId)
                      .HasColumnName("SubTaskId").
                       ValueGeneratedOnAdd();

                entity.Property(e => e.SubTaskName)
                      .HasColumnName("SubTaskName");

                entity.Property(e => e.TaskId)
                      .HasColumnName("TaskId");

                entity.HasOne(e => e.Task)
                      .WithMany(t => t.SubTasks)
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
