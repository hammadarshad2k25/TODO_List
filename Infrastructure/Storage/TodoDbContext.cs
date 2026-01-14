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
    }
}
