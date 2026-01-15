using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class DELETE_TASK : Endpoint<DeleteTaskRequest>
    {
        private readonly TodoDbContext _db;
        //private readonly IRedisService _redis;
        private readonly IOutputCacheStore _cachestore;
        private readonly ILogger<DELETE_TASK> _logger;
        public DELETE_TASK(TodoDbContext db, /*IRedisService redis,*/ IOutputCacheStore cachestore, ILogger<DELETE_TASK> logger)
        {
            _db = db;
            //_redis = redis;
            _cachestore = cachestore;
            _logger = logger;
        }
        public override void Configure()
        {
            Delete("/api/DELETE_TASK");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<DeleteTaskValidation>();
            Options(o => o.CacheOutput(p =>
            {
                p.NoCache();
            }));
        }
        public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            _logger.LogInformation("DeleteTask started. TaskId={TaskId}, UserId={UserId}", req.tid, UserId);
            var task = await _db.Tasks.Include(st => st.SubTasks).FirstOrDefaultAsync(t => t.TaskId == req.tid, ct);
            if (task == null)
            {
                _logger.LogWarning("DeleteTask failed. Task not found in SQL Server. TaskId={TaskId}", req.tid);
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
                return;
            }
            if (task.SubTasks.Any())
            {
                _db.RemoveRange(task.SubTasks);
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("SubTasks deleted from SQL Server. TaskId={TaskId}, Count={Count}", req.tid, task.SubTasks.Count);
            }
            await _db.Database.ExecuteSqlRawAsync("EXEC DELETETASK @TaskId = {0}", req.tid);
            _logger.LogInformation("Task deleted from SQL Server. TaskId={TaskId}", req.tid);
            var cacheKey = $"task:{req.tid}";
            try
            {
                //await _redis.DeleteAsync(cacheKey);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Redis cache removed Failed. CacheKey={CacheKey}", cacheKey);
            }
            _logger.LogInformation("Redis cache removed. CacheKey={CacheKey}", cacheKey);
            await _cachestore.EvictByTagAsync("task", ct);
            _logger.LogInformation("Output cache evicted by tag. Tag=task");
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            _logger.LogInformation("DeleteTask completed successfully. TaskId={TaskId}", req.tid);
            await HttpContext.Response.WriteAsync($"Task with ID {req.tid} has been deleted successfully.", cancellationToken: ct);
        }
    }
}
