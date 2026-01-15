using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;
using TODO_List.Domain.Model;
//using TODO_List.Infrastructure.Services;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class ADD_TASK : Endpoint<AddTaskRequest>
    {
        private readonly TodoDbContext _db;
        private readonly IRedisService _redis;
        //private readonly ElasticService _elastic;
        private readonly ILogger<ADD_TASK> _logger;
        public ADD_TASK(TodoDbContext db, IRedisService redis, /*ElasticService elastic*/ ILogger<ADD_TASK> logger)
        {
            _db = db;
            _redis = redis;
            //_elastic = elastic;
            _logger = logger;
        }
        public override void Configure()
        {
            Post("/api/ADD_TASK");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<AddTaskValidation>();
        }
        public override async Task HandleAsync(AddTaskRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            _logger.LogInformation("AddTask request started. TaskId={TaskId}, UserId={UserId}", req.tid, userId);
            var task = new TaskDbModel
            {
                TaskId = req.tid,
                TitleName = req.tname,
                IsCompleted = req.tisCompleted,
                SubTasks = req.subTasks?.Select(st => new SubTaskDbModel
                {
                    SubTaskId = st.subTaskId,
                    SubTaskName = st.subTaskName,
                    TaskId = req.tid
                }).ToList() ?? new List<SubTaskDbModel>()
            };
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Task saved in SQL Server. TaskId={TaskId}", req.tid);
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
            try
            {
                //await _elastic.IndexOneTaskAsync(new ElasticIndexModel
                //{
                //    Id = req.tid.ToString(),
                //    Title = req.tname,
                //    Description = req.Description,
                //    Tags = req.Tags,
                //    CreatedAt = DateTime.UtcNow,
                //    IsCompleted = req.tisCompleted
                //});
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Task Failed To indexed in Elasticsearch. TaskId={TaskId}", req.tid);
            }
            _logger.LogInformation("Task indexed in Elasticsearch. TaskId={TaskId}", req.tid);
            try
            {
                await _redis.SetAsync($"task:{response.Tid}", response, TimeSpan.FromMinutes(20));
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Task Failed cached in Redis. CacheKey={CacheKey}", response.Tid);
            }
            _logger.LogInformation("Task cached in Redis. CacheKey={CacheKey}", response.Tid);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            _logger.LogInformation("AddTask completed successfully. TaskId={TaskId}", req.tid);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
