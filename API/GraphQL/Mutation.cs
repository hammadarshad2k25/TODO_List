using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TODO_List.Domain.Entities;
using TODO_List.Infrastructure.Storage;
using HotChocolate;
using HotChocolate.Types;

namespace TODO_List.API.GraphQL
{
    public class Mutation
    {
        public async Task<TaskDbModel?> AddTask([Service] TodoDbContext context, int id, string name, bool isCompleted)
        {
            var result = await context.Tasks.FromSqlRaw("EXEC ADDTASK @TaskId = {0}, @TitleName = {1}, @IsCompleted = {2}", id,name,isCompleted).ToListAsync();
            return result.FirstOrDefault();
        }
        public async Task<TaskDbModel?> UpdateTask([Service] TodoDbContext context, int id, string name, bool isCompleted)
        {
            var result = await context.Tasks.FromSqlRaw("EXEC UPDATETASK @TaskId = {0}, @TitleName = {1}, @IsCompleted = {2}", id,name,isCompleted).ToListAsync();
            return result.FirstOrDefault();
        }
        public async Task<bool> DeleteTask([Service] TodoDbContext context, int id)
        {
            var result = await context.Database.ExecuteSqlRawAsync("EXEC DELETETASK @TaskId = {0}", id);
            return result > 0;
        }
    }
}
