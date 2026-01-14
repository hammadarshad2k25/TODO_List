//using Amazon.DynamoDBv2.DataModel;
//using FastEndpoints;
//using Microsoft.Azure.Cosmos;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;
//using TODO_List.Infrastructure.Storage;

//namespace TODO_List.API.FastEndpoints
//{
//    public class GetAllTaskFastEndPoint : Endpoint<GetAllPaginatedRequest>
//    {
//        private readonly TodoDbContext _db;
//        private readonly IRedisService _redis;
//        private readonly IDynamoDBContext _context;
//        private readonly Container _contain;
//        private readonly ILogger<GetAllTaskFastEndPoint> _logger;
//        public GetAllTaskFastEndPoint(TodoDbContext db, IRedisService redis, IDynamoDBContext context, CosmosClient client, ILogger<GetAllTaskFastEndPoint> logger)
//        {
//            _db = db;
//            _redis = redis;
//            _context = context;
//            _contain = client.GetDatabase("TodoListDB").GetContainer("Tasks");
//            _logger = logger;
//        }
//        public override void Configure()
//        {
//            Get("/api/GetAllTaskPaginated");
//            AllowAnonymous();
//            Validator<GetTaskValidation>();
//            Options(o => o.CacheOutput(p =>
//            {
//                p.Expire(TimeSpan.FromMinutes(2));
//                p.SetVaryByQuery("page", "pageSize");
//                p.Tag("task");
//            }));
//        }
//        public override async Task HandleAsync(GetAllPaginatedRequest req,CancellationToken ct)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
//            var cachekey = $"task:page:{req.page}:size:{req.pageSize}";
//            _logger.LogInformation("GetAllTask started. Page={Page}, PageSize={PageSize}", req.page, req.pageSize);
//            var cachedtask = await _redis.GetAsync<GetAllPaginatedResponse<TaskModelDTO>>(cachekey);
//            if (cachedtask is not null)
//            {
//                _logger.LogInformation("Cache hit for GetAllTask. CacheKey={CacheKey}", cachekey);
//                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
//                return;
//            }
//            _logger.LogInformation("Cache miss for GetAllTask. Trying DynamoDB");
//            var useDynamoDB = await GetTasksFromDynamoDB(req, ct);
//            if (useDynamoDB.Count > 0)
//            {
//                var totalDynamo = await _context.ScanAsync<TodoTaskDynamo>(new List<ScanCondition>()).GetRemainingAsync(ct);
//                _logger.LogInformation("Tasks loaded from DynamoDB. Count={Count}", useDynamoDB.Count);
//                var responseDynamo = new GetAllPaginatedResponse<TaskModelDTO>
//                {
//                    Items = useDynamoDB,
//                    Page = req.page,
//                    PageSize = req.pageSize,
//                    TotalCount = totalDynamo.Count
//                };
//                await _redis.SetAsync(cachekey, responseDynamo, TimeSpan.FromMinutes(20));
//                _logger.LogInformation("DynamoDB response cached. CacheKey={CacheKey}", cachekey);
//                await HttpContext.Response.WriteAsJsonAsync(responseDynamo, cancellationToken: ct);
//                return;
//            }
//            _logger.LogInformation("No DynamoDB data found. Falling back to SQL Server");
//            int skip = (req.page - 1) * req.pageSize;
//            var total = await _db.Tasks.CountAsync(ct);
//            var data = await _db.Tasks.Include(st => st.SubTasks).OrderBy(t => t.TaskId).Skip(skip).Take(req.pageSize).Select(t => new TaskModelDTO
//            {
//                Tid = t.TaskId,
//                Tname = t.TitleName,
//                TisCompleted = t.IsCompleted,
//                subTasks = t.SubTasks.Select(st => new SubTaskModelDTO
//                {
//                    subTaskId = st.SubTaskId,
//                    subTaskName = st.SubTaskName
//                }).ToList()
//            }).ToListAsync(ct);
//            _logger.LogInformation("Tasks loaded from SQL Server. Count={Count}, Total={Total}", data.Count, total);
//            var response = new GetAllPaginatedResponse<TaskModelDTO>
//            {
//                Items = data,
//                Page = req.page,
//                PageSize = req.pageSize,
//                TotalCount = total
//            };
//            await _redis.SetAsync(cachekey,response,TimeSpan.FromMinutes(20));
//            _logger.LogInformation("SQL response cached. CacheKey={CacheKey}", cachekey);
//            _logger.LogInformation("GetAllTask completed successfully. Page={Page}", req.page);
//            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
//        }
//        private async Task<List<TaskModelDTO>> GetTasksFromDynamoDB(GetAllPaginatedRequest req, CancellationToken ct)
//        {
//            _logger.LogInformation("Scanning DynamoDB for tasks");
//            var search = _context.ScanAsync<TodoTaskDynamo>(new List<ScanCondition>());
//            var tasks = await search.GetRemainingAsync(ct);
//            _logger.LogInformation("DynamoDB scan completed. TotalItems={Total}", tasks.Count);
//            return tasks.OrderByDescending(x => x.CreatedAt).Skip((req.page - 1) * req.pageSize).Take(req.pageSize).Select(t => new TaskModelDTO
//            {
//                Tid = int.Parse(t.TaskId),
//                Tname = t.Title,
//                TisCompleted = t.IsCompleted,
//                subTasks = new List<SubTaskModelDTO>()
//            }).ToList();
//        }
//    }
//}
