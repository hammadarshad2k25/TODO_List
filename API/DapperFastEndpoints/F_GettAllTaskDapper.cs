//using Azure;
//using Dapper;
//using FastEndpoints;
//using StackExchange.Redis;
//using System.Data;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;

//namespace TODO_List.API.DapperFastEndpoints
//{
//    public class F_GettAllTaskDapper : Endpoint<GetAllPaginatedRequest>
//    {
//        private readonly IDbConnection _db;
//        private readonly IRedisService _redis;
//        public F_GettAllTaskDapper(IDbConnection db, IRedisService redis)
//        {
//            _db = db;
//            _redis = redis;
//        }
//        public override void Configure()
//        {
//            Get("/api/Dapper/GetAllTaskPaginated");
//            AllowAnonymous();
//            Validator<GetTaskValidation>();
//        }
//        public override async Task HandleAsync(GetAllPaginatedRequest req,CancellationToken ct)
//        {
//            var cachekey = $"task:page:{req.page}:size:{req.pageSize}";
//            var cachedtask = await _redis.GetAsync<GetAllPaginatedResponse<TaskModelDTO>>(cachekey);
//            if (cachedtask is not null)
//            {
//                await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
//                return;
//            }
//            int skip = (req.page - 1) * req.pageSize;
//            var totaltasks = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Tasks");
//            var gettask = @"SELECT * FROM Tasks ORDER BY TaskId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;";
//            var tasks = await _db.QueryAsync<TaskDbModel>(gettask, new { Skip = skip, PageSize = req.pageSize });
//            var taskids = tasks.Select(t => t.TaskId).ToList();
//            var getsubtask = @"SELECT * FROM SubTaskDbModel WHERE TaskId IN @TaskIds;";
//            var subtasks = await _db.QueryAsync<SubTaskDbModel>(getsubtask, new { TaskIds = taskids });
//            var tasklist = tasks.Select(t => new TaskModelDTO
//            {
//                Tid = t.TaskId,
//                Tname = t.TitleName,
//                TisCompleted = t.IsCompleted,
//                subTasks = subtasks.Where(st => st.TaskId == t.TaskId).Select(st => new SubTaskModelDTO
//                {
//                    subTaskId = st.SubTaskId,
//                    subTaskName = st.SubTaskName
//                }).ToList()
//            }).ToList();
//            var Response = new GetAllPaginatedResponse<TaskModelDTO>
//            {
//                Items = tasklist,
//                TotalCount = totaltasks,
//                Page = req.page,
//                PageSize = req.pageSize
//            };
//            await _redis.SetAsync(cachekey, Response, TimeSpan.FromMinutes(20));
//            await HttpContext.Response.WriteAsJsonAsync(Response, cancellationToken: ct);
//        }
//    }
//}
