//using Dapper;
//using FastEndpoints;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using System.Data;
//using TODO_List.Application.DTO;
//using TODO_List.Application.Interfaces;
//using TODO_List.Application.ValidatorFastEndpoint;
//using TODO_List.Domain.Entities;

//namespace TODO_List.API.DapperFastEndpoints
//{
//    public class F_UpdateTaskDapper : Endpoint<UpdateTaskRequest>
//    {
//        private readonly IDbConnection _db;
//        private readonly IRedisService _redis;

//        public F_UpdateTaskDapper(IDbConnection db, IRedisService redis)
//        {
//            _db = db;
//            _redis = redis;
//        }

//        public override void Configure()
//        {
//            Put("/api/Dapper/UpdateTask/{id:int}");
//            Roles("Admin");
//            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
//            Validator<UpdateTaskValidation>();
//        }
//        public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
//        {
//            int id = Route<int>("id");
//            string cacheKey = $"dapper_task:{id}";
//            var task = await _db.QueryFirstOrDefaultAsync<TaskDbModel>(
//                "SELECT * FROM Tasks WHERE TaskId = @Id", new { Id = id });
//            if (task == null)
//            {
//                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
//                await HttpContext.Response.WriteAsync($"Task not found with ID {id}", ct);
//                return;
//            }
//            await _db.ExecuteAsync(
//                "UPDATE Tasks SET TitleName=@TitleName, IsCompleted=@IsCompleted WHERE TaskId=@Id",
//                new { TitleName = req.tname, IsCompleted = req.tisCompleted, Id = id });
//            if (req.subTasks.Any())
//            {
//                var ids = req.subTasks.Select(x => x.subTaskId).ToArray();
//                await _db.ExecuteAsync(
//                    "DELETE FROM SubTaskDbModel WHERE TaskId=@TaskId AND SubTaskId NOT IN @Ids",
//                    new { TaskId = id, Ids = ids });
//            }
//            else
//            {
//                await _db.ExecuteAsync("DELETE FROM SubTaskDbModel WHERE TaskId=@TaskId", new { TaskId = id });
//            }
//            foreach (var st in req.subTasks)
//            {
//                var query = @"
//IF EXISTS(SELECT 1 FROM SubTaskDbModel WHERE SubTaskId=@SubTaskId)
//    UPDATE SubTaskDbModel SET SubTaskName=@SubTaskName WHERE SubTaskId=@SubTaskId;
//ELSE
//    INSERT INTO SubTaskDbModel (SubTaskId, SubTaskName, TaskId)
//    VALUES (@SubTaskId, @SubTaskName, @TaskId);";
//                await _db.ExecuteAsync(query, new
//                {
//                    SubTaskId = st.subTaskId,
//                    SubTaskName = st.subTaskName,
//                    TaskId = id
//                });
//            }
//            var updatedTask = await _db.QueryAsync<TaskModelDTO>(
//                @"SELECT t.TaskId AS Tid, t.TitleName AS Tname, t.IsCompleted AS TisCompleted,
//                        st.SubTaskId, st.SubTaskName
//                  FROM Tasks t
//                  LEFT JOIN SubTaskDbModel st ON t.TaskId = st.TaskId
//                  WHERE t.TaskId = @Id",
//                new { Id = id });
//            var response = new TaskModelDTO
//            {
//                Tid = id,
//                Tname = req.tname,
//                TisCompleted = req.tisCompleted,
//                subTasks = updatedTask
//                    .Where(x => x.SubTaskId != 0)
//                    .Select(x => new SubTaskModelDTO
//                    {
//                        subTaskId = x.SubTaskId,
//                        subTaskName = x.SubTaskName
//                    }).ToList()
//            };
//            await _redis.DeleteAsync(cacheKey);
//            await _redis.SetAsync(cacheKey, response, TimeSpan.FromMinutes(20));
//            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
//        }
//    }
//}
