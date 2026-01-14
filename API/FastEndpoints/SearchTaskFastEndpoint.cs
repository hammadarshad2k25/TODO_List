using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;
using TODO_List.Infrastructure.Storage;
using Microsoft.Azure.Cosmos;

namespace TODO_List.API.FastEndpoints
{
    public class SearchTaskFastEndpoint : Endpoint<SearchTaskRequest>
    {
        private readonly TodoDbContext _db;
        private readonly IRedisService _redis;
        private readonly IDynamoDBContext _dynamo;
        private readonly Container _contain;
        private readonly ILogger<SearchTaskFastEndpoint> _logger;
        public SearchTaskFastEndpoint(TodoDbContext db, IRedisService redis, IDynamoDBContext dynamo, CosmosClient client, ILogger<SearchTaskFastEndpoint> logger)
        {
            _db = db;
            _redis = redis;
            _dynamo = dynamo;
            _contain = client.GetDatabase("TodoListDB").GetContainer("Tasks");
            _logger = logger;
        }
        public override void Configure()
        {
            Get("/api/SearchTask");
            AllowAnonymous();
            Validator<SearchTaskValidation>();
            Options(o => o.CacheOutput(p =>
            {
                p.Expire(TimeSpan.FromMinutes(2));
                p.SetVaryByQuery("tid");
                p.Tag("task");
            }));
        }
        public override async Task HandleAsync(SearchTaskRequest req,CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            var cacheKey = $"task:{req.tid}";
            _logger.LogInformation("SearchTask started. TaskId={TaskId}, UserId={UserId}", req.tid, userId);
            var cachedtask = await _redis.GetAsync<TaskModelDTO>(cacheKey);
            if(cachedtask != null)
            {
                _logger.LogInformation("Cache hit (Redis). TaskId={TaskId}", req.tid);
                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
                return;
            }
            _logger.LogInformation("Cache miss (Redis). TaskId={TaskId}", req.tid);
            var dynamotask = await _dynamo.LoadAsync<TodoTaskDynamo>(userId, req.tid.ToString(), ct);
            if (dynamotask != null)
            {
                _logger.LogInformation("Task found in DynamoDB. TaskId={TaskId}", req.tid);
                var responseDynamo = new DynamoModelDTO
                {
                    Tid = int.Parse(dynamotask.TaskId),
                    Tname = dynamotask.Title,
                    TisCompleted = dynamotask.IsCompleted,
                    subTasks = new List<SubTaskModelDTO>()
                };
                await _redis.SetAsync(cacheKey, responseDynamo, TimeSpan.FromMinutes(20));
                _logger.LogInformation("Task cached from DynamoDB. CacheKey={CacheKey}", cacheKey);
                await HttpContext.Response.WriteAsJsonAsync(responseDynamo, cancellationToken: ct);
                return;
            }
            try
            {
                var cosmostask = await _contain.ReadItemAsync<TodoTaskCosmos>(id: req.tid.ToString(), partitionKey: new PartitionKey(userId), cancellationToken: ct);
                if (cosmostask.Resource != null)
                {
                    _logger.LogInformation("Task found in CosmosDB. TaskId={TaskId}", req.tid);
                    var CosTask = cosmostask.Resource;
                    var responseCosmos = new CosmosModelDTO
                    {
                        id = CosTask.id,
                        title = CosTask.Title,
                        isCompleted = CosTask.IsCompleted,
                        description = CosTask.Description,
                        createdDate = DateTime.UtcNow
                    };
                    await _redis.SetAsync(cacheKey, responseCosmos, TimeSpan.FromMinutes(20));
                    _logger.LogInformation("Task cached from CosmosDB. CacheKey={CacheKey}", cacheKey);
                    await HttpContext.Response.WriteAsJsonAsync(responseCosmos, cancellationToken: ct);
                    return;
                }
            }
            catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Task not found in CosmosDB. TaskId={TaskId}", req.tid);
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
