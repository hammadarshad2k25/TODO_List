using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class SEARCH_TASK : Endpoint<SearchTaskRequest>
    {
        private readonly TodoDbContext _db;
        private readonly IRedisService _redis;
        private readonly ILogger<SEARCH_TASK> _logger;
        public SEARCH_TASK(TodoDbContext db, IRedisService redis, ILogger<SEARCH_TASK> logger)
        {
            _db = db;
            _redis = redis;
            _logger = logger;
        }
        public override void Configure()
        {
            Get("/api/SEARCH_TASK");
            AllowAnonymous();
            Validator<SearchTaskValidation>();
            Options(o => o.CacheOutput(p =>
            {
                p.Expire(TimeSpan.FromMinutes(2));
                p.SetVaryByQuery("tid");
                p.Tag("task");
            }));
        }
        public override async Task HandleAsync(SearchTaskRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            var cacheKey = $"task:{req.tid}";
            _logger.LogInformation("SearchTask started. TaskId={TaskId}, UserId={UserId}", req.tid, userId);
            TaskModelDTO? cachedtask = null;    
            try
            {
                cachedtask = await _redis.GetAsync<TaskModelDTO>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache miss (Redis). TaskId={TaskId}", req.tid);
            }
            if (cachedtask != null)
            {
                _logger.LogInformation("Cache hit (Redis). TaskId={TaskId}", req.tid);
                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
                return;
            }
            
            var task = await _db.Tasks.Include(st => st.SubTasks).FirstOrDefaultAsync(t => t.TaskId == req.tid, ct);
            if (task == null)
            {
                _logger.LogWarning("Task not found in SQL Server. TaskId={TaskId}", req.tid);

                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
                return;
            }
            _logger.LogInformation("Task found in SQL Server. TaskId={TaskId}", req.tid);
            var response = new TaskModelDTO
            {
                Tid = task.TaskId,
                Tname = task.TitleName,
                TisCompleted = task.IsCompleted,
                subTasks = task.SubTasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            };
            await _redis.SetAsync(cacheKey, response, TimeSpan.FromMinutes(20));
            _logger.LogInformation("Task cached from SQL Server. CacheKey={CacheKey}", cacheKey);
            _logger.LogInformation("SearchTask completed successfully. TaskId={TaskId}", req.tid);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
