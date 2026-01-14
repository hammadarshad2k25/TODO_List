using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;
using TODO_List.Domain.Model;
using TODO_List.Infrastructure.Services;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.API.FastEndpoints
{
    public class AddTaskFastEndPoint : Endpoint<AddTaskRequest>
    {
        private readonly TodoDbContext _db;
        private readonly IRedisService _redis;
        private readonly ElasticService _elastic;
        private readonly IDynamoDBContext _dynamo;
        private readonly Container _contain;
        private readonly ILogger<AddTaskFastEndPoint> _logger;
        public AddTaskFastEndPoint(TodoDbContext db, IRedisService redis, ElasticService elastic, IDynamoDBContext dynamo, CosmosClient client, ILogger<AddTaskFastEndPoint> logger)
        {
            _db = db;
            _redis = redis;
            _elastic = elastic;
            _dynamo = dynamo;
            _contain = client.GetDatabase("TodoListDB").GetContainer("Tasks");
            _logger = logger;
        }
        public override void Configure()
        {
            Post("/api/AddTask");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<AddTaskValidation>();
        }
        public override async Task HandleAsync(AddTaskRequest req,CancellationToken ct)
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
            var todoDynamo = new TodoTaskDynamo
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin",
                TaskId = req.tid.ToString(),
                Title = req.tname,
                Description = req.Description,
                IsCompleted = req.tisCompleted,
                CreatedAt = DateTime.UtcNow
            };
            await _dynamo.SaveAsync(todoDynamo,ct);
            _logger.LogInformation("Task saved in DynamoDB. TaskId={TaskId}", req.tid);
            var todoCosmos = new TodoTaskCosmos
            {
                id = req.tid.ToString(),
                UserId = userId,
                Title = req.tname,
                IsCompleted = req.tisCompleted,
                Description = req.Description,
                CreatedDate = DateTime.UtcNow
            };
            await _contain.CreateItemAsync(todoCosmos, new PartitionKey(todoCosmos.UserId),cancellationToken: ct);
            _logger.LogInformation("Task saved in CosmosDB. TaskId={TaskId}", req.tid);
            await _elastic.IndexOneTaskAsync(new ElasticIndexModel
            {
                Id = req.tid.ToString(),
                Title = req.tname,
                Description = req.Description,
                Tags = req.Tags,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = req.tisCompleted
            });
            _logger.LogInformation("Task indexed in Elasticsearch. TaskId={TaskId}", req.tid);
            await _redis.SetAsync($"task:{response.Tid}", response, TimeSpan.FromMinutes(20));
            _logger.LogInformation("Task cached in Redis. CacheKey={CacheKey}", response.Tid);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            _logger.LogInformation("AddTask completed successfully. TaskId={TaskId}", req.tid);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
