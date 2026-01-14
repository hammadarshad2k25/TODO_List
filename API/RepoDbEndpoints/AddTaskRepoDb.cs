using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RepoDb;
using System.Data;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Application.DTO;
using TODO_List.Domain.Entities;
using TODO_List.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using TODO_List.API.Hubs;
using TODO_List.Infrastructure.Services;
using TODO_List.Domain.Model;

namespace TODO_List.API.RepoDbEndpoints
{
    public class AddTaskRepoDb : Endpoint<AddTaskRequest>
    {
        private readonly IDbConnection _db;
        private readonly IRedisService _redis;
        private readonly IHubContext<TaskHub> _hub;
        private readonly ElasticService _elastic;
        public AddTaskRepoDb(IDbConnection db, IRedisService redis, IHubContext<TaskHub> hub, ElasticService elastic)
        {
            _db = db;
            _redis = redis;
            _hub = hub;
            _elastic = elastic;
        }
        public override void Configure()
        {
            Post("/api/RepoDb/AddTask");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<AddTaskValidation>();
        }
        public override async Task HandleAsync(AddTaskRequest req, CancellationToken ct)
        {
            var task = new TaskDbModel
            {
                TaskId = req.tid,
                TitleName = req.tname,
                IsCompleted = req.tisCompleted
            };
            await _db.InsertAsync(task);
            var subtasklist = new List<SubTaskModelDTO>();
            if (req.subTasks != null && req.subTasks.Any())
            {
                foreach (var st in req.subTasks)
                {
                    var subtask = new SubTaskDbModel
                    {
                        SubTaskId = st.subTaskId,
                        SubTaskName = st.subTaskName,
                        TaskId = task.TaskId
                    };
                    await _db.InsertAsync(subtask);
                    subtasklist.Add(new SubTaskModelDTO
                    {
                        subTaskId = st.subTaskId,
                        subTaskName = st.subTaskName
                    });
                }
            }
            var response = new TaskModelDTO
            {
                Tid = task.TaskId,
                Tname = task.TitleName,
                TisCompleted  = task.IsCompleted,
                subTasks = subtasklist
            };
            await _elastic.IndexOneTaskAsync(new ElasticIndexModel
            {
                Id = req.tid.ToString(),
                Title = req.tname,
                Description = req.Description,
                Tags = req.Tags,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = req.tisCompleted
            });
            var cachekey = $"repodb_task:{task.TaskId}";
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await _hub.Clients.All.SendAsync("TaskCreated", response, ct);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsync("Task and SubTasks added successfully.", ct);
        }
    }
}
