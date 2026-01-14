using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Dapper;
using System.Data;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Infrastructure.Services;
using TODO_List.Domain.Model;

namespace TODO_List.API.DapperFastEndpoints
{
    public class F_AddTaskDapper : Endpoint<AddTaskRequest>
    {
        private readonly IDbConnection _db;
        private readonly IRedisService _redis;
        private readonly ElasticService _elastic;
        public F_AddTaskDapper(IDbConnection db, IRedisService redis,ElasticService elastic)
        {
            _db = db;
            _redis = redis;
            _elastic = elastic;
        }
        public override void Configure()
        {
            Post("/api/Dapper/AddTask");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<AddTaskValidation>();
        }
        public override async Task HandleAsync(AddTaskRequest req, CancellationToken ct)
        {
            var addtask = "INSERT INTO Tasks (TaskId, TitleName, IsCompleted) VALUES (@TaskId, @TitleName, @IsCompleted);";
            await _db.ExecuteAsync(addtask, new 
            { 
               TaskId = req.tid, 
               TitleName = req.tname, 
               IsCompleted = req.tisCompleted 
            });
            if (req.subTasks != null && req.subTasks.Any())
            {
                var addsubtask = "INSERT INTO SubTaskDbModel (SubTaskId, SubTaskName, TaskId) VALUES (@SubTaskId, @SubTaskName, @TaskId);";
                foreach (var st in req.subTasks)
                {
                    await _db.ExecuteAsync(addsubtask, new 
                    { 
                       SubTaskId = st.subTaskId, 
                       SubTaskName = st.subTaskName, 
                       TaskId = req.tid 
                    });
                }
            }
            var response = new TaskModelDTO
            {
                Tid = req.tid,
                Tname = req.tname,
                TisCompleted = req.tisCompleted,
                subTasks = req.subTasks?.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.subTaskId,
                    subTaskName = st.subTaskName
                }).ToList() ?? new List<SubTaskModelDTO>()
            };
            await _elastic.IndexOneTaskAsync(new ElasticIndexModel
            {
                Id = req.tid.ToString(),
                Title = req.tname,
                Tags = req.Tags,
                Description = req.Description,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = req.tisCompleted
            });
            await _redis.SetAsync($"dapper_task:{response.Tid}", response, TimeSpan.FromMinutes(20));
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
