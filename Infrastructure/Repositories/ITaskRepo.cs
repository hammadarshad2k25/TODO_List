//using Microsoft.EntityFrameworkCore;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Infrastructure.Storage;
//using TODO_List.Domain.Model;
////using TODO_List.Infrastructure.Services;

//namespace TODO_List.Infrastructure.Repositories
//{
//    public class TaskRepo : ITaskRepo
//    {
//        private readonly TodoDbContext _context;
//        public TaskRepo(TodoDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<TaskModelDTO> AddTask(TodoModel task)
//        {
//            var result = await _context.Tasks.FromSqlRaw("EXEC ADDTASK @TaskId = {0}, @TitleName = {1}, @IsCompleted = {2}", task.Tid, task.Tname, task.TisCompleted).ToListAsync();
//            var addtask = result.FirstOrDefault();
//            if (addtask == null)
//            {
//                throw new Exception("Failed To Add The Task via Stored Procedure!");
//            }
//            return new TaskModelDTO
//            {
//                Tid = addtask.TaskId,
//                Tname = addtask.TitleName,
//                TisCompleted = addtask.IsCompleted,
//            };
//        }
//        public async Task<List<TaskModelDTO>> GetAllTasks()
//        {
//            var result = await _context.Tasks.FromSqlRaw("EXEC GETTASK").ToListAsync();
//            return result.Select(t => new TaskModelDTO
//            {
//                Tid = t.TaskId,
//                Tname = t.TitleName,
//                TisCompleted = t.IsCompleted
//            }).ToList();
//        }
//        public async Task<TaskModelDTO?> SearchTaskById(int TaskID)
//        {
//            var result = await _context.Tasks.FromSqlRaw("EXEC SEARCHTASK @TaskId = {0}", TaskID).ToListAsync();
//            var searchtask = result.FirstOrDefault();
//            if (searchtask == null)
//            {
//                return null;
//            }
//            return new TaskModelDTO
//            {
//                Tid = searchtask.TaskId,
//                Tname = searchtask.TitleName,
//                TisCompleted = searchtask.IsCompleted
//            };
//        }
//        public async Task<TaskModelDTO?> UpdateTaskById(int TaskID, TodoModel task)
//        {
//            var result = await _context.Tasks.FromSqlRaw("EXEC UPDATETASK @TaskId = {0}, @TitleName = {1}, @IsCompleted = {2}", TaskID, task.Tname, task.TisCompleted).ToListAsync();
//            var updatetask = result.FirstOrDefault();
//            if (updatetask == null)
//            {
//                return null;
//            }
//            return new TaskModelDTO
//            {
//                Tid = updatetask.TaskId,
//                Tname = updatetask.TitleName,
//                TisCompleted = updatetask.IsCompleted
//            };
//        }
//        public async Task<bool> DeleteTaskById(int TaskID)
//        {
//            var result = await _context.Database.ExecuteSqlRawAsync("EXEC DELETETASK @TaskId = {0}", TaskID);
//            return result > 0;
//        }
//    }
//}
