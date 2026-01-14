using Dapper;
using FastEndpoints;
using NHibernate.Cache;
using System.Data;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;

namespace TODO_List.API.DapperFastEndpoints
{
    public class F_SearchTaskDapper : Endpoint<SearchTaskRequest>
    {
        private readonly IDbConnection _db;
        private readonly IRedisService _redis;
        public F_SearchTaskDapper(IDbConnection db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }
        public override void Configure()
        {
            Get("/api/Dapper/SearchTask");
            AllowAnonymous();
            Validator<SearchTaskValidation>();
        }
        public override async Task HandleAsync(SearchTaskRequest req, CancellationToken ct)
        {
            var cachekey = $"dapper_task:{req.tid}";
            var cachedtask = await _redis.GetAsync<TaskModelDTO>(cachekey);
            if (cachedtask is not null)
            {
                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
                return;
            }
            var gettask = "SELECT * FROM Tasks WHERE TaskId = @TaskId;";
            var task = await _db.QueryFirstOrDefaultAsync<TaskDbModel>(gettask, new { TaskId = req.tid });
            if (task == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
                return;
            }
            var getsubtasks = "SELECT * FROM SubTaskDbModel WHERE TaskId = @TaskId;";
            var subtasks = await _db.QueryAsync<SubTaskDbModel>(getsubtasks, new { TaskId = req.tid });
            var response = new TaskModelDTO
            {
                Tid = task.TaskId,
                Tname = task.TitleName,
                TisCompleted = task.IsCompleted,
                subTasks = subtasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            };
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
