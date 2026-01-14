using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NHibernate.Linq;
using TODO_List.Application.DTO;
using TODO_List.Application.Interfaces;
using TODO_List.Application.ValidatorFastEndpoint;
using TODO_List.Domain.Model;

namespace TODO_List.API.NHibernateFastEndpoints
{
    public class NHF_UpdateTask : Endpoint<UpdateTaskRequest>
    {
        private readonly NHibernate.ISession _session;
        private readonly IRedisService _redis;
        public NHF_UpdateTask(NHibernate.ISession session, IRedisService redis)
        {
            _session = session;
            _redis = redis;
        }
        public override void Configure()
        {
            Put("/api/NHibernate/UpdateTask/{id:int}");
            Roles("Admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Validator<UpdateTaskValidation>();
        }
        public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
        {
            int id = Route<int>("id");
            var cachekey = $"nh_task:{id}";
            using var tx = _session.BeginTransaction();
            var task = await _session.Query<NH_TaskDbModel>()
                                     .FetchMany(t => t.SubTasks)
                                     .Where(t => t.TaskId == id)
                                     .SingleOrDefaultAsync(ct);
            if (task == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsync($"Task not found with the ID {id}", ct);
                return;
            }
            task.TitleName = req.tname;
            task.IsCompleted = req.tisCompleted;
            var reqSubTaskIds = req.subTasks.Select(x => x.subTaskId).ToList();
            var existingSubtasks = task.SubTasks.ToList();
            foreach (var oldSub in existingSubtasks)
            {
                if (!reqSubTaskIds.Contains(oldSub.SubTaskId))
                {
                    task.SubTasks.Remove(oldSub); 
                }
            }
            foreach (var st in req.subTasks)
            {
                var existing = task.SubTasks.FirstOrDefault(x => x.SubTaskId == st.subTaskId);
                if (existing != null)
                {
                    existing.SubTaskName = st.subTaskName;
                }
                else
                {
                    var newSubTask = new NH_SubTaskDbModel
                    {
                        SubTaskId = st.subTaskId,
                        SubTaskName = st.subTaskName,
                        Task = task
                    };
                    task.SubTasks.Add(newSubTask);
                }
            }
            await _session.SaveOrUpdateAsync(task, ct);
            await tx.CommitAsync(ct);  
            var response = new TaskModelDTO
            {
                Tid = task.TaskId,
                Tname = task.TitleName,
                TisCompleted = task.IsCompleted,
                subTasks = task.SubTasks.Select(st => new SubTaskModelDTO
                {
                    subTaskId = st.SubTaskId,
                    subTaskName = st.SubTaskName
                }).ToList()
            };
            await _redis.DeleteAsync(cachekey);
            await _redis.SetAsync(cachekey, response, TimeSpan.FromMinutes(20));
            await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: ct);
        }
    }
}
