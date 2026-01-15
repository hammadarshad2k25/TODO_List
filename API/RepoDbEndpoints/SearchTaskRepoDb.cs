//using FastEndpoints;
//using Microsoft.Data.SqlClient;
//using RepoDb;
//using StackExchange.Redis;
//using System.Data;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;

//namespace TODO_List.API.RepoDbEndpoints
//{
//    public class SearchTaskRepoDb : Endpoint<SearchTaskRequest>
//    {
//        private readonly IDbConnection _db;
//        //private readonly IRedisService _redis;
//        public SearchTaskRepoDb(IDbConnection db/*, IRedisService redis*/)
//        {
//            _db = db;
//            //_redis = redis;
//        }
//        public override void Configure()
//        {
//            Get("/api/RepoDb/SearchTask");
//            AllowAnonymous();
//            Validator<SearchTaskValidation>();
//        }
//        public override async Task HandleAsync(SearchTaskRequest req, CancellationToken ct)
//        {
//            var cachekey = $"repodb_task:{req.tid}";
//            var task = await _db.QueryAsync<TaskDbModel>(s => s.TaskId == req.tid);
//            var taskResult = task.FirstOrDefault();
//            if (taskResult == null)
//            {
//                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
//                return;
//            }
//            var subTasks = await _db.QueryAsync<SubTaskDbModel>(s => s.TaskId == req.tid);
//            var response = new TaskModelDTO
//            {
//                Tid = taskResult.TaskId,
//                Tname = taskResult.TitleName,
//                TisCompleted = taskResult.IsCompleted,
//                subTasks = subTasks.Select(st => new SubTaskModelDTO
//                {
//                    subTaskId = st.SubTaskId,
//                    subTaskName = st.SubTaskName
//                }).ToList()
//            };
//            //await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
//            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
//        }
//    }
//}
