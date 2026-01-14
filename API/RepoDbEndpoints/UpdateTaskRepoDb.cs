using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using RepoDb;
using StackExchange.Redis;
using System.Data;
using TODO_List.API.Hubs;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Entities;

namespace TODO_List.API.RepoDbEndpoints
{
    public class UpdateTaskRepoDb : Endpoint<UpdateTaskRequest>
    {
        private readonly IDbConnection _db;
        private readonly IRedisService _redis;
        private readonly IHubContext<TaskHub> _hub;
        public UpdateTaskRepoDb(IDbConnection db, IRedisService redis, IHubContext<TaskHub> hub)
        {
            _db = db;
            _redis = redis;
            _hub = hub;
        }
        public override void Configure()
        {
            Put("/api/RepoDb/UpdateTask/{id:int}");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<UpdateTaskValidation>();
        }
        public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
        {
            int id = Route<int>("id");
            var cachekey = $"repodb_task:{id}";
            var task = await _db.QueryAsync<TaskDbModel>(s => s.TaskId == id);
            var taskResult = task.FirstOrDefault();
            if (taskResult == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"Task not found with the ID {id}", ct);
                return;
            }
            var updateTask = new TaskDbModel
            {
                TaskId = id,
                TitleName = req.tname,
                IsCompleted = req.tisCompleted
            };
            await _db.UpdateAsync(updateTask);
            var existingSubTasks = await _db.QueryAsync<SubTaskDbModel>(s => s.TaskId == id);
            var existingSubTaskIds = existingSubTasks.Select(st => st.SubTaskId).ToList();
            var reqSubTaskIds = req.subTasks.Select(st => st.subTaskId).ToList();
            foreach (var oldSubTask in existingSubTasks)
            {
                if (!reqSubTaskIds.Contains(oldSubTask.SubTaskId))
                {
                    await _db.DeleteAsync(oldSubTask);
                }
            }
            foreach (var subTask in req.subTasks)
            {
                if (existingSubTaskIds.Contains(subTask.subTaskId))
                {
                    var updateSubTask = new SubTaskDbModel
                    {
                        SubTaskId = subTask.subTaskId,
                        SubTaskName = subTask.subTaskName,
                        TaskId = id
                    };
                    await _db.UpdateAsync(updateSubTask);
                }
                else
                {
                    var newSubTask = new SubTaskDbModel
                    {
                        SubTaskId = subTask.subTaskId,
                        SubTaskName = subTask.subTaskName,
                        TaskId = id
                    };
                    await _db.InsertAsync(newSubTask);
                }
            }
            var updatedsubTask = await _db.QueryAsync<SubTaskDbModel>(st => st.TaskId == id);
            var response = new TaskModelDTO
            {
                Tid = id,
                Tname = req.tname,
                TisCompleted = req.tisCompleted,
                subTasks = updatedsubTask.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            };
            await _redis.DeleteAsync(cachekey);
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await _hub.Clients.All.SendAsync("TaskUpdated", response, ct);
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
