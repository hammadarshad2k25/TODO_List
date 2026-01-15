//using FastEndpoints;
//using Microsoft.Data.SqlClient;
//using RepoDb;
//using System.Data;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;

//namespace TODO_List.API.RepoDbEndpoints
//{
//    public class GetTaskRepoDB : Endpoint<GetAllPaginatedRequest>
//    {
//        private readonly IDbConnection _db;
//        //private readonly IRedisService _redis;

//        public GetTaskRepoDB(IDbConnection db /*IRedisService redis*/)
//        {
//            _db = db;
//            //_redis = redis;
//        }
//        public override void Configure()
//        {
//            Get("/api/RepoDb/GetAllTaskPaginated");
//            AllowAnonymous();
//            Validator<GetTaskValidation>();
//        }
//        public override async Task HandleAsync(GetAllPaginatedRequest req, CancellationToken ct)
//        {
//            var cachekey = $"Task:Page:{req.page}:Size:{req.pageSize}";
//            ///*var cachedtask = await _redis.GetAsync<GetAllPaginatedResponse<TaskMo*/delDTO>>(cachekey);
//            //if (cachedtask is not null)
//            //{
//            //    await HttpContext.Response.WriteAsJsonAsync(cachedtask, cancellationToken: ct);
//            //    return;
//            //}
//            int skip = (req.page - 1) * req.pageSize;
//            var total = await _db.CountAsync<TaskDbModel>((object)null!, null, null, null, null, null, null, ct);
//            var tasks = await _db.ExecuteQueryAsync<TaskDbModel>("SELECT * FROM Tasks ORDER BY TaskId OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
//            new { Skip = skip, Take = req.pageSize });
//            var taskIds = tasks.Select(t => t.TaskId).ToList();
//            var subTasks = await _db.QueryAsync<SubTaskDbModel>(t => taskIds.Contains(t.TaskId));
//            var taskDtoList = tasks.Select(t => new TaskModelDTO
//            {
//                Tid = t.TaskId,
//                Tname = t.TitleName,
//                TisCompleted = t.IsCompleted,
//                subTasks = subTasks.Where(st => st.TaskId == t.TaskId).Select(st => new SubTaskModelDTO
//                {
//                    subTaskId = st.SubTaskId,
//                    subTaskName = st.SubTaskName
//                }).ToList()
//            }).ToList();
//            var response = new GetAllPaginatedResponse<TaskModelDTO>
//            {
//                Items = taskDtoList,
//                Page = req.page,
//                PageSize = req.pageSize,
//                TotalCount = (int)total
//            };
//            //await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
//            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
//        }
//    }
//}

