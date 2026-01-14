using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TODO_List.Domain.Entities;
using TODO_List.Infrastructure.Storage;
using HotChocolate;
using HotChocolate.Types;

namespace TODO_List.API.GraphQL
{
    public class Query
    {
        public async Task<List<TaskDbModel>> GetTasks([Service] TodoDbContext context)
        {
            var result = await context.Tasks.FromSqlRaw("EXEC GETTASK").ToListAsync();
            return result;
        }
        public async Task<TaskDbModel?> GetTaskById([Service] TodoDbContext context, int id)
        {
            var result = await context.Tasks.FromSqlRaw("EXEC SEARCHTASK @TaskId = {0}", id).ToListAsync();
            return result.FirstOrDefault();
        }
    }
}
