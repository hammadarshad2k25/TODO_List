//using Dapper;
//using FastEndpoints;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using System.Data;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;

//namespace TODO_List.API.DapperFastEndpoints
//{
//    public class F_DeleteTaskDapper : Endpoint<DeleteTaskRequest>
//    {
//        private readonly IDbConnection _db;
//        private readonly IRedisService _redis;
//        public F_DeleteTaskDapper(IDbConnection db, IRedisService redis)
//        {
//            _db = db;
//            _redis = redis;
//        }
//        public override void Configure()
//        {
//            Delete("/api/Dapper/DeleteTask");
//            Roles("Admin");
//            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
//            Validator<DeleteTaskValidation>();
//        }
//        public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
//        {
//            var cachekey = $"dapper_task:{req.tid}";
//            var task = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Tasks WHERE TaskId = @TaskId", new { TaskId = req.tid });
//            if (task == 0)
//            {
//                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//                await HttpContext.Response.WriteAsync($"Task not found with the ID {req.tid}", ct);
//                return;
//            }
//            await _db.ExecuteAsync("DELETE FROM Tasks WHERE TaskId = @Id", new { Id = req.tid });
//            await _db.ExecuteAsync("DELETE FROM SubTaskDbModel WHERE TaskId = @TaskId", new { TaskId = req.tid });
//            await _redis.DeleteAsync(cachekey);
//            await HttpContext.Response.WriteAsync($"Task with ID {req.tid} has been deleted successfully.", cancellationToken: ct);
//        }
//    }
//}
