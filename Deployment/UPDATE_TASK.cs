using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;
using TODO_List.Domain.Model;
using TODO_List.Infrastructure.Services;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class UPDATE_TASK : Endpoint<UpdateTaskRequest>
    {
        private readonly TodoDbContext _db;
        private readonly IRedisService _redis;
        private readonly ElasticService _service;
        private readonly IOutputCacheStore _cachestore;
        private readonly ILogger<UPDATE_TASK> _logger;
        public UPDATE_TASK(TodoDbContext db, IRedisService redis, ElasticService service, IOutputCacheStore cachestore, ILogger<UPDATE_TASK> logger)
        {
            _db = db;
            _redis = redis;
            _service = service;
            _cachestore = cachestore;
            _logger = logger;
        }
        public override void Configure()
        {
            Put("/api/UPDATE_TASK/{id:int}");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<UpdateTaskValidation>();
            Options(o => o.CacheOutput(p =>
            {
                p.NoCache();
            }));
        }
        public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
        {
            int id = Route<int>("id");
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            _logger.LogInformation("UpdateTask started. TaskId={TaskId}, UserId={UserId}", id, UserId);
            var task = await _db.Tasks.Include(st => st.SubTasks).FirstOrDefaultAsync(t => t.TaskId == id, ct);
            if (task == null)
            {
                _logger.LogWarning("UpdateTask failed. Task not found. TaskId={TaskId}", id);
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"Task not found with the ID {id}", ct);
                return;
            }
            await _db.Database.ExecuteSqlRawAsync("EXEC UPDATETASK @TaskId = {0}, @TitleName = {1}, @IsCompleted = {2}", id, req.tname, req.tisCompleted);
            await _db.Entry(task).ReloadAsync(ct);
            _logger.LogInformation("Task updated in SQL Server. TaskId={TaskId}", id);
            var existSubTask = task.SubTasks.ToList();
            var reqSubTaskIds = req.subTasks.Select(st => st.subTaskId).ToList();
            foreach (var oldSubTask in existSubTask)
            {
                if (!reqSubTaskIds.Contains(oldSubTask.SubTaskId))
                {
                    task.SubTasks.Remove(oldSubTask);
                }
            }
            foreach (var subTask in req.subTasks)
            {
                var existingSubTask = task.SubTasks.FirstOrDefault(st => st.SubTaskId == subTask.subTaskId);
                if (existingSubTask != null)
                {
                    existingSubTask.SubTaskName = subTask.subTaskName;
                }
                else
                {
                    task.SubTasks.Add(new SubTaskDbModel
                    {
                        SubTaskId = subTask.subTaskId,
                        SubTaskName = subTask.subTaskName,
                        TaskId = id
                    });
                }
            }
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SubTasks Updates Successfully. TaskId={TaskId}, Count={Count}", id, task.SubTasks.Count);
            try
            {
                await _service.IndexOneTaskAsync(new ElasticIndexModel
                {
                    Id = id.ToString(),
                    Title = req.tname,
                    Description = req.Description,
                    Tags = req.Tags,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = req.tisCompleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Task Failed To re-indexed in Elasticsearch. TaskId={TaskId}", id);
            }
            _logger.LogInformation("Task Successfully re-indexed in Elasticsearch. TaskId={TaskId}", id);
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
            await _cachestore.EvictByTagAsync("task", ct);
            var cacheKey = $"task:{id}";
            try
            {
                await _redis.SetAsync(cacheKey, response, TimeSpan.FromMinutes(20));
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Cache Failed To refreshed. CacheKey={CacheKey}", cacheKey);
            }
            _logger.LogInformation("Cache refreshed. CacheKey={CacheKey}", cacheKey);
            _logger.LogInformation("UpdateTask completed successfully. TaskId={TaskId}", id);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
