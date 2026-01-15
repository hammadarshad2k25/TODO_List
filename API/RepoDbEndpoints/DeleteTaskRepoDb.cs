//using FastEndpoints;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.SignalR;
//using RepoDb;
//using StackExchange.Redis;
//using System.Data;
//using System.Threading.Tasks;
//using TODO_List.API.Hubs;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;

//namespace TODO_List.API.RepoDbEndpoints
//{
//    public class DeleteTaskRepoDb : Endpoint<DeleteTaskRequest>
//    {
//        private readonly IDbConnection _db;
//        //private readonly IRedisService _redis;
//        private readonly IHubContext<TaskHub> _hub;
//        public DeleteTaskRepoDb(IDbConnection db, /*IRedisService redis,*/ IHubContext<TaskHub> hub)
//        {
//            _db = db;
//            //_redis = redis;
//            _hub = hub;
//        }
//        public override void Configure()
//        {
//            Delete("/api/RepoDb/DeleteTask");
//            Roles("Admin");
//            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
//            Validator<DeleteTaskValidation>();
//        }
//        public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
//        {
//            var cachekey = $"repodb_task:{req.tid}";
//            var tasks = (await _db.QueryAsync<TaskDbModel>(t => t.TaskId == req.tid)).FirstOrDefault();
//            if (tasks == null)
//            {
//                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//                await HttpContext.Response.WriteAsync($"No tasks found with ID {req.tid}.", ct);
//                return;
//            }
//            var subtasks = await _db.QueryAsync<SubTaskDbModel>(st => st.TaskId == req.tid);
//            if(subtasks.Any())
//            {
//                foreach(var st in subtasks)
//                {
//                    await _db.DeleteAsync<SubTaskDbModel>(st.SubTaskId);
//                }
//            }
//            await _db.ExecuteNonQueryAsync("EXEC DELETETASK @TaskId", new { TaskId = req.tid });
//            //await _redis.DeleteAsync(cachekey);
//            await _hub.Clients.All.SendAsync("TaskDeleted", req.tid, ct);
//            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
//            await HttpContext.Response.WriteAsync($"Task with ID {req.tid} has been deleted successfully.", cancellationToken: ct);
//        }
//    }
//}
