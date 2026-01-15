using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Infrastructure.Storage;

namespace TODO_List.Deployment
{
    public class GET_ALL_TASK : Endpoint<GetAllPaginatedRequest>
    {
        private readonly TodoDbContext _db;
        //private readonly IRedisService _redis;
        private readonly ILogger<GET_ALL_TASK> _logger;
        public GET_ALL_TASK(TodoDbContext db, /*IRedisService redis,*/ ILogger<GET_ALL_TASK> logger)
        {
            _db = db;
            //_redis = redis;
            _logger = logger;
        }
        public override void Configure()
        {
            Get("/api/Get_ALL_TASK");
            AllowAnonymous();
            Validator<GetTaskValidation>();
            Options(o => o.CacheOutput(p =>
            {
                p.Expire(TimeSpan.FromMinutes(2));
                p.SetVaryByQuery("page", "pageSize");
                p.Tag("task");
            }));
        }
        public override async Task HandleAsync(GetAllPaginatedRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            var cachekey = $"task:page:{req.page}:size:{req.pageSize}";
            _logger.LogInformation("GetAllTask started. Page={Page}, PageSize={PageSize}", req.page, req.pageSize);
            //GetAllPaginatedResponse<TaskModelDTO>? cachedtask = null;
            try
            {
                //cachedtask = await _redis.GetAsync<GetAllPaginatedResponse<TaskModelDTO>>(cachekey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache miss for GetAllTask. Trying SQL Server");
            }
            //if (cachedtask is not null)
            //{
            //    _logger.LogInformation("Cache hit for GetAllTask. CacheKey={CacheKey}", cachekey);
            //    await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
            //    return;
            //}
            int skip = (req.page - 1) * req.pageSize;
            var total = await _db.Tasks.CountAsync(ct);
            var data = await _db.Tasks.Include(st => st.SubTasks).OrderBy(t => t.TaskId).Skip(skip).Take(req.pageSize).Select(t => new TaskModelDTO
            {
                Tid = t.TaskId,
                Tname = t.TitleName,
                TisCompleted = t.IsCompleted,
                subTasks = t.SubTasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            }).ToListAsync(ct);
            _logger.LogInformation("Tasks loaded from SQL Server. Count={Count}, Total={Total}", data.Count, total);
            var response = new GetAllPaginatedResponse<TaskModelDTO>
            {
                Items = data,
                Page = req.page,
                PageSize = req.pageSize,
                TotalCount = total
            };
            //await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            _logger.LogInformation("SQL response cached. CacheKey={CacheKey}", cachekey);
            _logger.LogInformation("GetAllTask completed successfully. Page={Page}", req.page);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
